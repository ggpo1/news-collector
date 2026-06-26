using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Options;
using NewsCollector.Domain.Entities;
using NewsCollector.Domain.Enums;
using NewsCollector.Infrastructure.Persistence;
using NewsCollector.Infrastructure.Search;

namespace NewsCollector.Infrastructure.Services;

public sealed class NewsSearchIndexService : INewsSearchIndexService
{
    private readonly NewsCollectorDbContext _db;
    private readonly IContentLanguageResolver _languageResolver;
    private readonly NewsSearchIndexerOptions _options;
    private readonly ILogger<NewsSearchIndexService> _logger;

    public NewsSearchIndexService(
        NewsCollectorDbContext db,
        IContentLanguageResolver languageResolver,
        IOptions<NewsSearchIndexerOptions> options,
        ILogger<NewsSearchIndexService> logger)
    {
        _db = db;
        _languageResolver = languageResolver;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> IndexPendingNewsAsync(CancellationToken cancellationToken = default)
    {
        var pendingItems = await _db.NewsItems
            .Include(item => item.Source)
            .Where(item => item.SearchIndexedAt == null)
            .OrderByDescending(item => item.PublishedAt ?? item.FetchedAt)
            .ThenByDescending(item => item.FetchedAt)
            .ThenByDescending(item => item.CreatedAt)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        if (pendingItems.Count == 0)
        {
            return 0;
        }

        var indexedAt = DateTimeOffset.UtcNow;
        var indexedCount = 0;

        foreach (var item in pendingItems)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await UpsertNewsDocumentAsync(item, indexedAt, cancellationToken);
                item.Language = _languageResolver.Resolve(
                    item.Source,
                    item.Title,
                    item.Summary,
                    item.Content);
                item.SearchIndexedAt = indexedAt;
                indexedCount++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Failed to index news {NewsId} for search", item.Id);
            }

            if (_options.DelayBetweenItemsMs > 0)
            {
                await Task.Delay(_options.DelayBetweenItemsMs, cancellationToken);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        if (indexedCount > 0)
        {
            _logger.LogInformation("Indexed {Count} news items for search", indexedCount);
        }

        return indexedCount;
    }

    private async Task UpsertNewsDocumentAsync(
        NewsItem news,
        DateTimeOffset indexedAt,
        CancellationToken cancellationToken)
    {
        var language = _languageResolver.Resolve(
            news.Source,
            news.Title,
            news.Summary,
            news.Content);
        var body = SearchTextBuilder.BuildNewsBody(news, _options.MaxBodyCharacters);

        var document = await _db.SearchDocuments
            .FirstOrDefaultAsync(
                doc => doc.DocumentType == SearchDocumentType.News && doc.EntityId == news.Id,
                cancellationToken);

        if (document is null)
        {
            document = new SearchDocument
            {
                Id = Guid.NewGuid(),
                DocumentType = SearchDocumentType.News,
                EntityId = news.Id,
                Title = news.Title,
            };
            _db.SearchDocuments.Add(document);
        }

        document.Language = language;
        document.Title = news.Title;
        document.Body = body;
        document.SourceName = news.Source.Name;
        document.PublishedAt = news.PublishedAt ?? news.FetchedAt;
        document.UpdatedAt = indexedAt;
    }
}

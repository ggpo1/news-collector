using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Application.Models;
using NewsCollector.Application.Options;
using NewsCollector.Domain.Entities;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class ArticleContentEnrichmentService : IArticleContentEnrichmentService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly NewsCollectorDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHtmlContentExtractor _contentExtractor;
    private readonly INewsQueryService _newsQueryService;
    private readonly CollectorOptions _options;
    private readonly ILogger<ArticleContentEnrichmentService> _logger;

    public ArticleContentEnrichmentService(
        NewsCollectorDbContext db,
        IHttpClientFactory httpClientFactory,
        IHtmlContentExtractor contentExtractor,
        INewsQueryService newsQueryService,
        IOptions<CollectorOptions> options,
        ILogger<ArticleContentEnrichmentService> logger)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _contentExtractor = contentExtractor;
        _newsQueryService = newsQueryService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> EnrichPendingArticlesAsync(CancellationToken cancellationToken = default)
    {
        var pendingItems = await _db.NewsItems
            .Include(n => n.Source)
            .Where(n => n.Content == null && n.ContentFetchedAt == null)
            .OrderByDescending(n => n.PublishedAt)
            .Take(_options.ContentEnrichmentBatchSize)
            .ToListAsync(cancellationToken);

        if (pendingItems.Count == 0)
        {
            return 0;
        }

        var httpClient = _httpClientFactory.CreateClient("ArticleFetcher");
        var enrichedCount = 0;

        foreach (var item in pendingItems)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await TryEnrichItemAsync(item, httpClient, cancellationToken))
            {
                enrichedCount++;
            }

            if (_options.ContentEnrichmentDelayMs > 0)
            {
                await Task.Delay(_options.ContentEnrichmentDelayMs, cancellationToken);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        if (enrichedCount > 0)
        {
            _logger.LogInformation("Enriched {EnrichedCount} articles with full text", enrichedCount);
        }

        return enrichedCount;
    }

    public async Task<ArticleEnrichmentResult> EnrichArticleAsync(
        Guid newsId,
        CancellationToken cancellationToken = default)
    {
        var item = await _db.NewsItems
            .Include(n => n.Source)
            .FirstOrDefaultAsync(n => n.Id == newsId, cancellationToken);

        if (item is null)
        {
            return new ArticleEnrichmentResult(ArticleEnrichmentResult.StatusNotFound, null, null);
        }

        if (!string.IsNullOrWhiteSpace(item.Content))
        {
            var existing = await _newsQueryService.GetByIdAsync(newsId, cancellationToken);
            return new ArticleEnrichmentResult(
                ArticleEnrichmentResult.StatusAlreadyEnriched,
                existing,
                null);
        }

        var scrapingConfig = ParseScrapingConfig(item.Source.Config);
        if (!scrapingConfig.ContentFetchEnabled || string.IsNullOrWhiteSpace(scrapingConfig.ContentSelector))
        {
            return new ArticleEnrichmentResult(
                ArticleEnrichmentResult.StatusNotSupported,
                await MapDetailAsync(newsId, cancellationToken),
                "Content fetching is not configured for this source");
        }

        var httpClient = _httpClientFactory.CreateClient("ArticleFetcher");
        var enriched = await TryEnrichItemAsync(item, httpClient, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var detail = await MapDetailAsync(newsId, cancellationToken);

        if (enriched)
        {
            _logger.LogInformation("Manually enriched article {NewsId}", newsId);
            return new ArticleEnrichmentResult(ArticleEnrichmentResult.StatusEnriched, detail, null);
        }

        return new ArticleEnrichmentResult(
            ArticleEnrichmentResult.StatusFailed,
            detail,
            "Failed to fetch or extract article content");
    }

    private async Task<bool> TryEnrichItemAsync(
        NewsItem item,
        HttpClient httpClient,
        CancellationToken cancellationToken)
    {
        var scrapingConfig = ParseScrapingConfig(item.Source.Config);
        if (!scrapingConfig.ContentFetchEnabled || string.IsNullOrWhiteSpace(scrapingConfig.ContentSelector))
        {
            item.ContentFetchedAt = DateTimeOffset.UtcNow;
            return false;
        }

        try
        {
            var html = await httpClient.GetStringAsync(item.Url, cancellationToken);
            var content = await _contentExtractor.ExtractAsync(
                html,
                scrapingConfig.ContentSelector,
                cancellationToken);

            item.Content = content;
            item.ContentFetchedAt = DateTimeOffset.UtcNow;

            if (!string.IsNullOrWhiteSpace(content))
            {
                _logger.LogDebug("Fetched full text for {NewsId} ({Length} chars)", item.Id, content.Length);
                return true;
            }

            _logger.LogWarning("Empty article content extracted for {Url}", item.Url);
            return false;
        }
        catch (Exception ex)
        {
            item.ContentFetchedAt = DateTimeOffset.UtcNow;
            _logger.LogWarning(ex, "Failed to fetch article content from {Url}", item.Url);
            return false;
        }
    }

    private Task<NewsItemDetailDto?> MapDetailAsync(Guid newsId, CancellationToken cancellationToken) =>
        _newsQueryService.GetByIdAsync(newsId, cancellationToken);

    private static SourceScrapingConfig ParseScrapingConfig(string? configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return new SourceScrapingConfig { ContentFetchEnabled = false };
        }

        try
        {
            return JsonSerializer.Deserialize<SourceScrapingConfig>(configJson, JsonOptions)
                ?? new SourceScrapingConfig { ContentFetchEnabled = false };
        }
        catch (JsonException)
        {
            return new SourceScrapingConfig { ContentFetchEnabled = false };
        }
    }
}

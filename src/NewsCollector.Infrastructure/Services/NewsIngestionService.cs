using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Models;
using NewsCollector.Domain.Entities;
using NewsCollector.Domain.Enums;
using NewsCollector.Infrastructure.Feeds;
using NewsCollector.Infrastructure.Persistence;
using Npgsql;

namespace NewsCollector.Infrastructure.Services;

public sealed class NewsIngestionService : INewsIngestionService
{
    private readonly NewsCollectorDbContext _db;
    private readonly IRssFeedReader _rssFeedReader;
    private readonly ILogger<NewsIngestionService> _logger;

    public NewsIngestionService(
        NewsCollectorDbContext db,
        IRssFeedReader rssFeedReader,
        ILogger<NewsIngestionService> logger)
    {
        _db = db;
        _rssFeedReader = rssFeedReader;
        _logger = logger;
    }

    public async Task<int> CollectPendingSourcesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var sources = await _db.Sources
            .Where(s => s.IsActive)
            .Where(s => s.LastFetchedAt == null
                || s.LastFetchedAt.Value.AddMinutes(s.FetchIntervalMinutes) <= now)
            .ToListAsync(cancellationToken);

        var totalNew = 0;

        foreach (var source in sources)
        {
            try
            {
                totalNew += await CollectFromSourceAsync(source, cancellationToken);
            }
            catch (Exception ex) when (IsRssTimeout(ex))
            {
                _logger.LogWarning(
                    "RSS fetch timed out for source {SourceName} ({SourceId}); will retry after fetch interval",
                    source.Name,
                    source.Id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(
                    ex,
                    "RSS HTTP error for source {SourceName} ({SourceId}); will retry after fetch interval",
                    source.Name,
                    source.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to collect news from source {SourceName} ({SourceId}); will retry after fetch interval",
                    source.Name,
                    source.Id);
            }
        }

        return totalNew;
    }

    private async Task<int> CollectFromSourceAsync(Source source, CancellationToken cancellationToken)
    {
        var fetchedAt = DateTimeOffset.UtcNow;

        try
        {
            if (source.Type != SourceType.Rss)
            {
                _logger.LogWarning("Source type {SourceType} is not supported yet for {SourceName}", source.Type, source.Name);
                return 0;
            }

            _logger.LogInformation("Collecting news from {SourceName}", source.Name);

            var feedItems = FeedItemDeduplicator.Deduplicate(await _rssFeedReader.ReadAsync(source.Url, cancellationToken));

            if (feedItems.Count == 0)
            {
                return 0;
            }

            var externalIds = feedItems.Select(i => i.ExternalId).ToList();
            var urls = feedItems.Select(i => i.Url).ToList();

            var existingKeys = await _db.NewsItems
                .AsNoTracking()
                .Where(n => n.SourceId == source.Id
                    && (externalIds.Contains(n.ExternalId) || urls.Contains(n.Url)))
                .Select(n => new { n.ExternalId, n.Url })
                .ToListAsync(cancellationToken);

            var existingExternalIds = existingKeys
                .Select(x => x.ExternalId)
                .ToHashSet(StringComparer.Ordinal);

            var existingUrls = existingKeys
                .Select(x => x.Url)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var candidates = feedItems
                .Where(item => !existingExternalIds.Contains(item.ExternalId) && !existingUrls.Contains(item.Url))
                .Select(item => MapNewsItem(item, source.Id, fetchedAt))
                .ToList();

            var insertedCount = await InsertNewsItemsAsync(candidates, cancellationToken);

            _logger.LogInformation(
                "Collected {NewCount} new items from {SourceName} ({TotalInFeed} in feed, {SkippedDuplicates} skipped as duplicates)",
                insertedCount,
                source.Name,
                feedItems.Count,
                feedItems.Count - candidates.Count);

            return insertedCount;
        }
        finally
        {
            // Back off failing/slow feeds: respect FetchIntervalMinutes before the next attempt.
            await UpdateSourceFetchTimestampAsync(source, fetchedAt, cancellationToken);
        }
    }

    private static NewsItem MapNewsItem(ParsedFeedItem item, Guid sourceId, DateTimeOffset fetchedAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            SourceId = sourceId,
            ExternalId = item.ExternalId.Trim(),
            Title = item.Title,
            Summary = item.Summary,
            Url = item.Url.Trim(),
            PublishedAt = item.PublishedAt,
            FetchedAt = fetchedAt,
            ContentHash = RssFeedReader.ComputeContentHash(item.Title, item.Summary),
            RawPayload = item.RawPayload,
            CreatedAt = fetchedAt
        };

    private async Task<int> InsertNewsItemsAsync(
        IReadOnlyList<NewsItem> candidates,
        CancellationToken cancellationToken)
    {
        if (candidates.Count == 0)
        {
            return 0;
        }

        try
        {
            _db.NewsItems.AddRange(candidates);
            await _db.SaveChangesAsync(cancellationToken);
            return candidates.Count;
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            _logger.LogWarning(
                "Bulk insert hit duplicate keys, falling back to per-item insert for {Count} items",
                candidates.Count);

            return await InsertNewsItemsOneByOneAsync(candidates, cancellationToken);
        }
    }

    private async Task<int> InsertNewsItemsOneByOneAsync(
        IReadOnlyList<NewsItem> candidates,
        CancellationToken cancellationToken)
    {
        var insertedCount = 0;

        foreach (var item in candidates)
        {
            try
            {
                _db.NewsItems.Add(item);
                await _db.SaveChangesAsync(cancellationToken);
                insertedCount++;
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                _logger.LogDebug(
                    "Skipped duplicate news item for source {SourceId}: {ExternalId}",
                    item.SourceId,
                    item.ExternalId);
            }
            finally
            {
                _db.Entry(item).State = EntityState.Detached;
            }
        }

        return insertedCount;
    }

    private async Task UpdateSourceFetchTimestampAsync(
        Source source,
        DateTimeOffset fetchedAt,
        CancellationToken cancellationToken)
    {
        source.LastFetchedAt = fetchedAt;
        source.UpdatedAt = fetchedAt;
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    private static bool IsRssTimeout(Exception exception) =>
        exception is TaskCanceledException or OperationCanceledException
            && (exception.InnerException is TimeoutException
                || exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase)
                || exception.Message.Contains("canceled", StringComparison.OrdinalIgnoreCase));
}

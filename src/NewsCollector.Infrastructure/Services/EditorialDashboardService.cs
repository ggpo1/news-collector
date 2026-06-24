using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Application.Options;
using NewsCollector.Domain.Entities;
using NewsCollector.Domain.Enums;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class EditorialDashboardService : IEditorialDashboardService
{
    private readonly NewsCollectorDbContext _db;
    private readonly EditorialDashboardOptions _options;

    public EditorialDashboardService(
        NewsCollectorDbContext db,
        IOptions<EditorialDashboardOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public async Task<EditorialDashboardDto> GetDashboardAsync(
        int? windowHours = null,
        CancellationToken cancellationToken = default)
    {
        var hours = windowHours is > 0 and <= 168 ? windowHours.Value : _options.WindowHours;
        var now = DateTimeOffset.UtcNow;
        var windowStart = now.AddHours(-hours);
        var previousWindowStart = windowStart.AddHours(-hours);

        var newsItems = await _db.NewsItems
            .AsNoTracking()
            .Include(item => item.Source)
            .Include(item => item.Category)
            .Where(item => (item.PublishedAt ?? item.FetchedAt) >= windowStart)
            .ToListAsync(cancellationToken);

        var newsById = newsItems.ToDictionary(item => item.Id);
        var newsIds = newsById.Keys.ToHashSet();

        var links = newsIds.Count == 0
            ? []
            : await _db.NewsLinks
                .AsNoTracking()
                .Where(link => newsIds.Contains(link.NewsIdLow) && newsIds.Contains(link.NewsIdHigh))
                .Where(link => link.LinkType == LinkType.SameTopic || link.LinkType == LinkType.Duplicate)
                .ToListAsync(cancellationToken);

        var clusters = EditorialNewsClusterBuilder.BuildClusters(newsIds, links);
        var developingTopicKeys = new HashSet<string>();

        var developingTopics = clusters
            .Select(cluster => BuildClusterDto(cluster, newsById, links))
            .Where(cluster => cluster is not null)
            .Cast<ClusterSnapshot>()
            .Where(cluster => cluster.SourceCount >= _options.MinSourcesForDevelopingTopic)
            .OrderByDescending(cluster => cluster.SourceCount)
            .ThenByDescending(cluster => cluster.ArticleCount)
            .Take(_options.MaxDevelopingTopics)
            .Select(cluster =>
            {
                developingTopicKeys.Add(cluster.ClusterKey);
                return new DevelopingTopicDto(
                    cluster.ClusterKey,
                    cluster.Headline,
                    cluster.SourceCount,
                    cluster.ArticleCount,
                    cluster.SourceNames,
                    cluster.Primary,
                    cluster.Related);
            })
            .ToList();

        var duplicateGroups = clusters
            .Select(cluster => BuildClusterDto(cluster, newsById, links))
            .Where(cluster => cluster is not null)
            .Cast<ClusterSnapshot>()
            .Where(cluster =>
                cluster.ArticleCount >= _options.MinArticlesForDuplicateGroup
                && !developingTopicKeys.Contains(cluster.ClusterKey))
            .OrderByDescending(cluster => cluster.HasDuplicateLink)
            .ThenByDescending(cluster => cluster.ArticleCount)
            .ThenByDescending(cluster => cluster.SourceCount)
            .Take(_options.MaxDuplicateGroups)
            .Select(cluster => new DuplicateGroupDto(
                cluster.ClusterKey,
                cluster.Headline,
                cluster.SourceCount,
                cluster.ArticleCount,
                cluster.HasDuplicateLink,
                cluster.SourceNames,
                cluster.Primary,
                cluster.Related))
            .ToList();

        var entitySpikes = await BuildEntitySpikesAsync(
            windowStart,
            previousWindowStart,
            now,
            cancellationToken);

        var toneHighlights = newsItems
            .Where(item => item.ToneCoefficient.HasValue && Math.Abs(item.ToneCoefficient.Value) >= _options.MinToneAbsolute)
            .OrderByDescending(item => Math.Abs(item.ToneCoefficient!.Value))
            .Take(_options.MaxToneHighlights)
            .Select(item => new ToneHighlightDto(
                MapNews(item),
                item.ToneCoefficient!.Value,
                DescribeTone(item.ToneCoefficient.Value)))
            .ToList();

        return new EditorialDashboardDto(
            developingTopics,
            duplicateGroups,
            entitySpikes,
            toneHighlights,
            new EditorialDashboardMetaDto(hours, windowStart, now, newsItems.Count));
    }

    private async Task<IReadOnlyList<EntitySpikeDto>> BuildEntitySpikesAsync(
        DateTimeOffset windowStart,
        DateTimeOffset previousWindowStart,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var currentCounts = await (
            from mention in _db.NewsEntityMentions.AsNoTracking()
            join news in _db.NewsItems.AsNoTracking() on mention.NewsItemId equals news.Id
            where (news.PublishedAt ?? news.FetchedAt) >= windowStart
                  && (news.PublishedAt ?? news.FetchedAt) <= now
            group mention by mention.NamedEntityId
            into grouped
            select new { EntityId = grouped.Key, Count = grouped.Count() })
            .ToListAsync(cancellationToken);

        if (currentCounts.Count == 0)
        {
            return [];
        }

        var previousCounts = await (
            from mention in _db.NewsEntityMentions.AsNoTracking()
            join news in _db.NewsItems.AsNoTracking() on mention.NewsItemId equals news.Id
            where (news.PublishedAt ?? news.FetchedAt) >= previousWindowStart
                  && (news.PublishedAt ?? news.FetchedAt) < windowStart
            group mention by mention.NamedEntityId
            into grouped
            select new { EntityId = grouped.Key, Count = grouped.Count() })
            .ToDictionaryAsync(item => item.EntityId, item => item.Count, cancellationToken);

        var spikes = currentCounts
            .Select(item =>
            {
                previousCounts.TryGetValue(item.EntityId, out var previousCount);
                var ratio = previousCount == 0
                    ? item.Count
                    : (double)item.Count / previousCount;

                return new
                {
                    item.EntityId,
                    item.Count,
                    PreviousCount = previousCount,
                    Ratio = ratio
                };
            })
            .Where(item =>
                item.Count >= _options.EntitySpikeMinMentions
                && item.Ratio >= _options.EntitySpikeMinRatio)
            .OrderByDescending(item => item.Ratio)
            .ThenByDescending(item => item.Count)
            .Take(_options.MaxEntitySpikes)
            .ToList();

        if (spikes.Count == 0)
        {
            return [];
        }

        var entityIds = spikes.Select(item => item.EntityId).ToList();
        var entities = await _db.NamedEntities
            .AsNoTracking()
            .Where(entity => entityIds.Contains(entity.Id))
            .ToDictionaryAsync(entity => entity.Id, cancellationToken);

        var recentArticlesByEntity = await LoadRecentArticlesForEntitiesAsync(
            entityIds,
            windowStart,
            cancellationToken);

        return spikes
            .Where(item => entities.ContainsKey(item.EntityId))
            .Select(item =>
            {
                var entity = entities[item.EntityId];
                recentArticlesByEntity.TryGetValue(item.EntityId, out var articles);
                return new EntitySpikeDto(
                    entity.Id,
                    entity.Name,
                    entity.Type.ToString(),
                    item.Count,
                    item.PreviousCount,
                    Math.Round(item.Ratio, 2),
                    articles ?? []);
            })
            .ToList();
    }

    private async Task<Dictionary<Guid, IReadOnlyList<EditorialBriefNewsDto>>> LoadRecentArticlesForEntitiesAsync(
        IReadOnlyList<Guid> entityIds,
        DateTimeOffset windowStart,
        CancellationToken cancellationToken)
    {
        var rows = await (
            from mention in _db.NewsEntityMentions.AsNoTracking()
            join news in _db.NewsItems.AsNoTracking() on mention.NewsItemId equals news.Id
            join source in _db.Sources.AsNoTracking() on news.SourceId equals source.Id
            join category in _db.Categories.AsNoTracking() on news.CategoryId equals category.Id into categories
            from category in categories.DefaultIfEmpty()
            where entityIds.Contains(mention.NamedEntityId)
                  && (news.PublishedAt ?? news.FetchedAt) >= windowStart
            orderby news.PublishedAt ?? news.FetchedAt descending
            select new
            {
                mention.NamedEntityId,
                News = news,
                SourceName = source.Name,
                CategoryName = category != null ? category.Name : null
            })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(row => row.NamedEntityId)
            .ToDictionary(
                group => group.Key,
                IReadOnlyList<EditorialBriefNewsDto> (group) => group
                    .DistinctBy(row => row.News.Id)
                    .Take(5)
                    .Select(row => MapNews(row.News, row.SourceName, row.CategoryName))
                    .ToList());
    }

    private ClusterSnapshot? BuildClusterDto(
        IReadOnlySet<Guid> clusterIds,
        IReadOnlyDictionary<Guid, NewsItem> newsById,
        IReadOnlyList<NewsLink> links)
    {
        var articles = clusterIds
            .Where(newsById.ContainsKey)
            .Select(id => newsById[id])
            .ToList();

        if (articles.Count == 0)
        {
            return null;
        }

        var ordered = articles
            .OrderByDescending(ScorePrimaryCandidate)
            .ThenBy(item => item.PublishedAt ?? item.FetchedAt)
            .ToList();

        var primary = MapNews(ordered[0]);
        var related = ordered
            .Skip(1)
            .Take(5)
            .Select(MapNews)
            .ToList();

        var sourceNames = articles
            .Select(item => item.Source.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var clusterKey = ordered.Min(item => item.Id).ToString("N");

        return new ClusterSnapshot(
            clusterKey,
            primary.Title,
            sourceNames.Count,
            articles.Count,
            sourceNames,
            EditorialNewsClusterBuilder.ClusterHasDuplicateLink(clusterIds, links),
            primary,
            related);
    }

    private static int ScorePrimaryCandidate(NewsItem item)
    {
        var score = 0;
        if (!string.IsNullOrWhiteSpace(item.Content))
        {
            score += 1_000;
        }

        if (item.ContentFetchedAt.HasValue)
        {
            score += 100;
        }

        if (!string.IsNullOrWhiteSpace(item.Summary))
        {
            score += 10;
        }

        return score;
    }

    private static EditorialBriefNewsDto MapNews(NewsItem item) =>
        MapNews(item, item.Source.Name, item.Category?.Name);

    private static EditorialBriefNewsDto MapNews(NewsItem item, string sourceName, string? categoryName) =>
        new(
            item.Id,
            item.Title,
            sourceName,
            categoryName,
            item.ToneCoefficient,
            item.PublishedAt,
            item.FetchedAt,
            !string.IsNullOrWhiteSpace(item.Content),
            item.Url);

    private static string DescribeTone(decimal tone) =>
        tone switch
        {
            >= 0.75m => "очень позитивно",
            >= 0.55m => "позитивно",
            <= -0.75m => "очень негативно",
            <= -0.55m => "негативно",
            _ => "нейтрально"
        };

    private sealed record ClusterSnapshot(
        string ClusterKey,
        string Headline,
        int SourceCount,
        int ArticleCount,
        IReadOnlyList<string> SourceNames,
        bool HasDuplicateLink,
        EditorialBriefNewsDto Primary,
        IReadOnlyList<EditorialBriefNewsDto> Related);
}

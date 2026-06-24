using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Options;
using NewsCollector.Domain.Entities;
using NewsCollector.Domain.Enums;
using NewsCollector.Infrastructure.Linking;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class StorySyncService : IStorySyncService
{
    private readonly NewsCollectorDbContext _db;
    private readonly TopicLinkerOptions _options;
    private readonly ILogger<StorySyncService> _logger;

    public StorySyncService(
        NewsCollectorDbContext db,
        IOptions<TopicLinkerOptions> options,
        ILogger<StorySyncService> logger)
    {
        _db = db;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> SyncFromLinksAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddHours(-_options.LookbackHours);

        var newsItems = await _db.NewsItems
            .AsNoTracking()
            .Include(item => item.Source)
            .Where(item => item.PublishedAt == null || item.PublishedAt >= cutoff)
            .ToListAsync(cancellationToken);

        if (newsItems.Count < 2)
        {
            return 0;
        }

        var newsById = newsItems.ToDictionary(item => item.Id);
        var newsIds = newsById.Keys.ToHashSet();

        var links = await _db.NewsLinks
            .AsNoTracking()
            .Where(link => newsIds.Contains(link.NewsIdLow) && newsIds.Contains(link.NewsIdHigh))
            .Where(link => link.LinkType == LinkType.SameTopic
                || link.LinkType == LinkType.Duplicate
                || link.LinkType == LinkType.Related)
            .ToListAsync(cancellationToken);

        var clusters = EditorialNewsClusterBuilder.BuildClusters(newsIds, links)
            .Where(cluster => cluster.Count >= 2)
            .ToList();

        if (clusters.Count == 0)
        {
            return 0;
        }

        var clusterKeys = clusters
            .Select(cluster => StoryClusterHelper.BuildClusterKey(cluster))
            .ToList();

        var existingStories = await _db.Stories
            .Include(story => story.NewsItems)
            .Where(story => clusterKeys.Contains(story.ClusterKey))
            .ToDictionaryAsync(story => story.ClusterKey, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var synced = 0;

        foreach (var cluster in clusters)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var clusterKey = StoryClusterHelper.BuildClusterKey(cluster);
            var articles = cluster
                .Where(newsById.ContainsKey)
                .Select(id => newsById[id])
                .ToList();

            if (articles.Count < 2)
            {
                continue;
            }

            var primary = StoryClusterHelper.SelectPrimary(articles);
            var sourceCount = articles.Select(item => item.SourceId).Distinct().Count();
            var firstSeen = articles.Min(item => item.PublishedAt ?? item.FetchedAt);
            var lastActivity = articles.Max(item => item.PublishedAt ?? item.FetchedAt);

            if (!existingStories.TryGetValue(clusterKey, out var story))
            {
                story = new Story
                {
                    Id = Guid.NewGuid(),
                    ClusterKey = clusterKey,
                    Title = primary.Title,
                    Status = StoryStatus.Monitoring,
                    PrimaryNewsItemId = primary.Id,
                    ArticleCount = articles.Count,
                    SourceCount = sourceCount,
                    FirstSeenAt = firstSeen,
                    LastActivityAt = lastActivity,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _db.Stories.Add(story);
                existingStories[clusterKey] = story;
            }
            else
            {
                story.Title = primary.Title;
                story.PrimaryNewsItemId = primary.Id;
                story.ArticleCount = articles.Count;
                story.SourceCount = sourceCount;
                story.FirstSeenAt = firstSeen;
                story.LastActivityAt = lastActivity;
                story.UpdatedAt = now;
            }

            var existingNewsIds = story.NewsItems.Select(item => item.NewsItemId).ToHashSet();
            foreach (var article in articles)
            {
                if (existingNewsIds.Contains(article.Id))
                {
                    continue;
                }

                story.NewsItems.Add(new StoryNewsItem
                {
                    StoryId = story.Id,
                    NewsItemId = article.Id,
                    LinkedAt = now
                });
            }

            synced++;
        }

        if (synced > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Synced {Count} stories from link clusters", synced);
        }

        return synced;
    }
}

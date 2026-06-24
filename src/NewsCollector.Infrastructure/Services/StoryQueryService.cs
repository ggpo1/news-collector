using Microsoft.EntityFrameworkCore;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Enums;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class StoryQueryService : IStoryQueryService
{
    private readonly NewsCollectorDbContext _db;

    public StoryQueryService(NewsCollectorDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<StoryListDto>> GetPagedAsync(
        int page,
        int pageSize,
        StoryStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Stories.AsNoTracking().AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(story => story.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var stories = await query
            .OrderByDescending(story => story.LastActivityAt ?? story.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (stories.Count == 0)
        {
            return new PagedResult<StoryListDto>([], page, pageSize, totalCount);
        }

        var storyIds = stories.Select(story => story.Id).ToList();
        var sourceNamesByStory = await LoadSourceNamesByStoryAsync(storyIds, cancellationToken);

        var items = stories
            .Select(story => new StoryListDto(
                story.Id,
                story.ClusterKey,
                story.Title,
                story.Status,
                story.ArticleCount,
                story.SourceCount,
                story.FirstSeenAt,
                story.LastActivityAt,
                story.UpdatedAt,
                sourceNamesByStory.GetValueOrDefault(story.Id, [])))
            .ToList();

        return new PagedResult<StoryListDto>(items, page, pageSize, totalCount);
    }

    public Task<StoryDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        LoadDetailAsync(story => story.Id == id, cancellationToken);

    public Task<StoryDetailDto?> GetByClusterKeyAsync(string clusterKey, CancellationToken cancellationToken = default) =>
        LoadDetailAsync(story => story.ClusterKey == clusterKey, cancellationToken);

    public async Task<Guid?> FindStoryIdForNewsAsync(Guid newsId, CancellationToken cancellationToken = default)
    {
        var storyId = await _db.StoryNewsItems
            .AsNoTracking()
            .Where(item => item.NewsItemId == newsId)
            .OrderByDescending(item => item.LinkedAt)
            .Select(item => item.StoryId)
            .FirstOrDefaultAsync(cancellationToken);

        return storyId == Guid.Empty ? null : storyId;
    }

    private async Task<StoryDetailDto?> LoadDetailAsync(
        System.Linq.Expressions.Expression<Func<Domain.Entities.Story, bool>> predicate,
        CancellationToken cancellationToken)
    {
        var story = await _db.Stories
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, cancellationToken);

        if (story is null)
        {
            return null;
        }

        var newsRows = await (
            from link in _db.StoryNewsItems.AsNoTracking()
            join news in _db.NewsItems.AsNoTracking() on link.NewsItemId equals news.Id
            join source in _db.Sources.AsNoTracking() on news.SourceId equals source.Id
            join category in _db.Categories.AsNoTracking() on news.CategoryId equals category.Id into categories
            from category in categories.DefaultIfEmpty()
            where link.StoryId == story.Id
            orderby news.PublishedAt ?? news.FetchedAt descending
            select new
            {
                news.Id,
                news.Title,
                SourceName = source.Name,
                CategoryName = category != null ? category.Name : null,
                news.ToneCoefficient,
                news.PublishedAt,
                news.FetchedAt,
                HasContent = news.Content != null,
                news.Url
            })
            .ToListAsync(cancellationToken);

        var newsIds = newsRows.Select(row => row.Id).ToList();

        var keyEntities = await (
            from mention in _db.NewsEntityMentions.AsNoTracking()
            join entity in _db.NamedEntities.AsNoTracking() on mention.NamedEntityId equals entity.Id
            where newsIds.Contains(mention.NewsItemId)
            group mention by new { entity.Id, entity.Name, entity.Type } into grouped
            orderby grouped.Count() descending
            select new StoryEntityDto(
                grouped.Key.Id,
                grouped.Key.Name,
                grouped.Key.Type.ToString(),
                grouped.Count()))
            .Take(12)
            .ToListAsync(cancellationToken);

        var rewrites = await (
            from rewrite in _db.NewsRewrites.AsNoTracking()
            join author in _db.Users.AsNoTracking() on rewrite.AuthorId equals author.Id
            join news in _db.NewsItems.AsNoTracking() on rewrite.SourceNewsId equals news.Id
            where newsIds.Contains(rewrite.SourceNewsId)
            orderby rewrite.UpdatedAt descending
            select new StoryRewriteDto(
                rewrite.Id,
                rewrite.SourceNewsId,
                news.Title,
                rewrite.Title,
                author.DisplayName,
                rewrite.UpdatedAt))
            .ToListAsync(cancellationToken);

        var deliveries = await (
            from delivery in _db.TelegramDeliveries.AsNoTracking()
            join channel in _db.TelegramChannels.AsNoTracking() on delivery.TelegramChannelId equals channel.Id
            where (delivery.NewsItemId != null && newsIds.Contains(delivery.NewsItemId.Value))
                || (delivery.NewsRewriteId != null && _db.NewsRewrites.Any(r =>
                    r.Id == delivery.NewsRewriteId && newsIds.Contains(r.SourceNewsId)))
            orderby delivery.CreatedAt descending
            select new StoryTelegramDeliveryDto(
                delivery.Id,
                channel.Name,
                delivery.Status.ToString(),
                delivery.CreatedAt,
                delivery.SentAt,
                delivery.NewsItemId,
                delivery.NewsRewriteId))
            .Take(30)
            .ToListAsync(cancellationToken);

        var timeline = newsRows
            .Select(row => new StoryTimelineItemDto(
                row.Id,
                row.Title,
                row.SourceName,
                row.CategoryName,
                row.ToneCoefficient,
                row.PublishedAt,
                row.FetchedAt,
                row.HasContent,
                row.Url,
                row.Id == story.PrimaryNewsItemId))
            .ToList();

        return new StoryDetailDto(
            story.Id,
            story.ClusterKey,
            story.Title,
            story.Status,
            story.ArticleCount,
            story.SourceCount,
            story.FirstSeenAt,
            story.LastActivityAt,
            story.CreatedAt,
            story.UpdatedAt,
            story.PrimaryNewsItemId,
            timeline,
            keyEntities,
            rewrites,
            deliveries);
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyList<string>>> LoadSourceNamesByStoryAsync(
        IReadOnlyList<Guid> storyIds,
        CancellationToken cancellationToken)
    {
        var rows = await (
            from link in _db.StoryNewsItems.AsNoTracking()
            join news in _db.NewsItems.AsNoTracking() on link.NewsItemId equals news.Id
            join source in _db.Sources.AsNoTracking() on news.SourceId equals source.Id
            where storyIds.Contains(link.StoryId)
            select new { link.StoryId, SourceName = source.Name })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(row => row.StoryId)
            .ToDictionary(
                group => group.Key,
                IReadOnlyList<string> (group) => group
                    .Select(row => row.SourceName)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .ToList());
    }
}

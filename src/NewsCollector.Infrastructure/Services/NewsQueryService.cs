using Microsoft.EntityFrameworkCore;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Application.Enums;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class NewsQueryService : INewsQueryService
{
    private readonly NewsCollectorDbContext _db;
    private readonly IStoryQueryService _storyQueryService;

    public NewsQueryService(NewsCollectorDbContext db, IStoryQueryService storyQueryService)
    {
        _db = db;
        _storyQueryService = storyQueryService;
    }

    public async Task<PagedResult<NewsItemListDto>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? sourceId = null,
        Guid? categoryId = null,
        bool? uncategorized = null,
        bool? hasContent = null,
        NewsToneFilter? toneFilter = null,
        Guid? editorialTagId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.NewsItems
            .AsNoTracking()
            .AsQueryable();

        if (sourceId.HasValue)
        {
            query = query.Where(n => n.SourceId == sourceId.Value);
        }

        if (uncategorized == true)
        {
            query = query.Where(n => n.CategoryId == null);
        }
        else if (categoryId.HasValue)
        {
            query = query.Where(n => n.CategoryId == categoryId.Value);
        }

        if (hasContent == true)
        {
            query = query.Where(n => n.Content != null);
        }
        else if (hasContent == false)
        {
            query = query.Where(n => n.Content == null);
        }

        query = ApplyToneFilter(query, toneFilter);

        if (editorialTagId.HasValue)
        {
            query = query.Where(news =>
                _db.NewsEditorialTags.Any(tag =>
                    tag.NewsItemId == news.Id && tag.EditorialTagId == editorialTagId.Value));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.PublishedAt)
            .ThenByDescending(n => n.FetchedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new
            {
                n.Id,
                n.SourceId,
                SourceName = n.Source.Name,
                CategoryName = n.Category != null ? n.Category.Name : null,
                n.ToneCoefficient,
                n.Title,
                n.Summary,
                n.Url,
                n.PublishedAt,
                n.FetchedAt,
                HasContent = n.Content != null
            })
            .ToListAsync(cancellationToken);

        var newsIds = items.Select(item => item.Id).ToList();
        var tagsByNewsId = await NewsItemDtoMapper.LoadTagsByNewsIdsAsync(_db, newsIds, cancellationToken);

        var mappedItems = items
            .Select(item => new NewsItemListDto(
                item.Id,
                item.SourceId,
                item.SourceName,
                item.CategoryName,
                item.ToneCoefficient,
                item.Title,
                item.Summary,
                item.Url,
                item.PublishedAt,
                item.FetchedAt,
                item.HasContent,
                tagsByNewsId.GetValueOrDefault(item.Id, [])))
            .ToList();

        return new PagedResult<NewsItemListDto>(mappedItems, page, pageSize, totalCount);
    }

    public async Task<NewsItemDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var news = await _db.NewsItems
            .AsNoTracking()
            .Where(n => n.Id == id)
            .Select(n => new
            {
                n.Id,
                n.SourceId,
                SourceName = n.Source.Name,
                n.CategoryId,
                CategoryName = n.Category != null ? n.Category.Name : null,
                n.IsCategoryManual,
                n.ToneCoefficient,
                n.ToneAnalyzedAt,
                n.ExternalId,
                n.Title,
                n.Summary,
                n.Content,
                n.Url,
                n.PublishedAt,
                n.FetchedAt,
                n.ContentFetchedAt,
                n.ContentHash,
                n.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (news is null)
        {
            return null;
        }

        var tags = await NewsItemDtoMapper.LoadTagsByNewsIdsAsync(_db, [id], cancellationToken);
        var storyId = await _storyQueryService.FindStoryIdForNewsAsync(id, cancellationToken);

        return new NewsItemDetailDto(
            news.Id,
            news.SourceId,
            news.SourceName,
            news.CategoryId,
            news.CategoryName,
            news.IsCategoryManual,
            news.ToneCoefficient,
            news.ToneAnalyzedAt,
            news.ExternalId,
            news.Title,
            news.Summary,
            news.Content,
            news.Url,
            news.PublishedAt,
            news.FetchedAt,
            news.ContentFetchedAt,
            news.ContentHash,
            news.CreatedAt,
            tags.GetValueOrDefault(id, []),
            storyId);
    }

    public async Task<IReadOnlyList<RelatedNewsDto>> GetRelatedAsync(
        Guid newsId,
        CancellationToken cancellationToken = default)
    {
        var links = await _db.NewsLinks
            .AsNoTracking()
            .Include(l => l.NewsLow).ThenInclude(n => n.Source)
            .Include(l => l.NewsLow).ThenInclude(n => n.Category)
            .Include(l => l.NewsHigh).ThenInclude(n => n.Source)
            .Include(l => l.NewsHigh).ThenInclude(n => n.Category)
            .Where(l => l.NewsIdLow == newsId || l.NewsIdHigh == newsId)
            .ToListAsync(cancellationToken);

        return links
            .Select(l =>
            {
                var related = l.NewsIdLow == newsId ? l.NewsHigh : l.NewsLow;
                return new RelatedNewsDto(
                    l.Id,
                    l.LinkType,
                    l.LinkMethod,
                    l.Confidence,
                    new NewsItemListDto(
                        related.Id,
                        related.SourceId,
                        related.Source.Name,
                        related.Category?.Name,
                        related.ToneCoefficient,
                        related.Title,
                        related.Summary,
                        related.Url,
                        related.PublishedAt,
                        related.FetchedAt,
                        related.Content != null,
                        []));
            })
            .ToList();
    }

    private static IQueryable<Domain.Entities.NewsItem> ApplyToneFilter(
        IQueryable<Domain.Entities.NewsItem> query,
        NewsToneFilter? toneFilter)
    {
        if (toneFilter is null)
        {
            return query;
        }

        return toneFilter switch
        {
            NewsToneFilter.Positive => query.Where(n => n.ToneCoefficient >= 0.3m),
            NewsToneFilter.Negative => query.Where(n => n.ToneCoefficient <= -0.3m),
            NewsToneFilter.Neutral => query.Where(n =>
                n.ToneCoefficient != null
                && n.ToneCoefficient > -0.3m
                && n.ToneCoefficient < 0.3m),
            NewsToneFilter.Strong => query.Where(n =>
                n.ToneCoefficient != null
                && (n.ToneCoefficient >= 0.55m || n.ToneCoefficient <= -0.55m)),
            NewsToneFilter.Unanalyzed => query.Where(n => n.ToneCoefficient == null),
            _ => query
        };
    }
}

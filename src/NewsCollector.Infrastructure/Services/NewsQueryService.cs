using Microsoft.EntityFrameworkCore;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class NewsQueryService : INewsQueryService
{
    private readonly NewsCollectorDbContext _db;

    public NewsQueryService(NewsCollectorDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<NewsItemListDto>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? sourceId = null,
        bool? hasContent = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.NewsItems
            .AsNoTracking()
            .Include(n => n.Source)
            .AsQueryable();

        if (sourceId.HasValue)
        {
            query = query.Where(n => n.SourceId == sourceId.Value);
        }

        if (hasContent == true)
        {
            query = query.Where(n => n.Content != null);
        }
        else if (hasContent == false)
        {
            query = query.Where(n => n.Content == null);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.PublishedAt)
            .ThenByDescending(n => n.FetchedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NewsItemListDto(
                n.Id,
                n.SourceId,
                n.Source.Name,
                n.Category != null ? n.Category.Name : null,
                n.Title,
                n.Summary,
                n.Url,
                n.PublishedAt,
                n.FetchedAt,
                n.Content != null))
            .ToListAsync(cancellationToken);

        return new PagedResult<NewsItemListDto>(items, page, pageSize, totalCount);
    }

    public async Task<NewsItemDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.NewsItems
            .AsNoTracking()
            .Include(n => n.Source)
            .Where(n => n.Id == id)
            .Select(n => new NewsItemDetailDto(
                n.Id,
                n.SourceId,
                n.Source.Name,
                n.Category != null ? n.Category.Name : null,
                n.ExternalId,
                n.Title,
                n.Summary,
                n.Content,
                n.Url,
                n.PublishedAt,
                n.FetchedAt,
                n.ContentFetchedAt,
                n.ContentHash,
                n.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
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
                        related.Title,
                        related.Summary,
                        related.Url,
                        related.PublishedAt,
                        related.FetchedAt,
                        related.Content != null));
            })
            .ToList();
    }
}

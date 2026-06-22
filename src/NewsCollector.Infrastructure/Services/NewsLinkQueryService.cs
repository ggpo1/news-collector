using Microsoft.EntityFrameworkCore;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Enums;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class NewsLinkQueryService : INewsLinkQueryService
{
    private readonly NewsCollectorDbContext _db;

    public NewsLinkQueryService(NewsCollectorDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<NewsLinkListDto>> GetPagedAsync(
        int page,
        int pageSize,
        LinkType? linkType = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.NewsLinks
            .AsNoTracking()
            .AsQueryable();

        if (linkType.HasValue)
        {
            query = query.Where(l => l.LinkType == linkType.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new NewsLinkListDto(
                l.Id,
                l.LinkType,
                l.LinkMethod,
                l.Confidence,
                l.CreatedAt,
                new NewsItemListDto(
                    l.NewsLow.Id,
                    l.NewsLow.SourceId,
                    l.NewsLow.Source.Name,
                    l.NewsLow.Category != null ? l.NewsLow.Category.Name : null,
                    l.NewsLow.ToneCoefficient,
                    l.NewsLow.Title,
                    l.NewsLow.Summary,
                    l.NewsLow.Url,
                    l.NewsLow.PublishedAt,
                    l.NewsLow.FetchedAt,
                    l.NewsLow.Content != null),
                new NewsItemListDto(
                    l.NewsHigh.Id,
                    l.NewsHigh.SourceId,
                    l.NewsHigh.Source.Name,
                    l.NewsHigh.Category != null ? l.NewsHigh.Category.Name : null,
                    l.NewsHigh.ToneCoefficient,
                    l.NewsHigh.Title,
                    l.NewsHigh.Summary,
                    l.NewsHigh.Url,
                    l.NewsHigh.PublishedAt,
                    l.NewsHigh.FetchedAt,
                    l.NewsHigh.Content != null)))
            .ToListAsync(cancellationToken);

        return new PagedResult<NewsLinkListDto>(items, page, pageSize, totalCount);
    }
}

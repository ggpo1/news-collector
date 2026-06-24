using NewsCollector.Application.Dtos;
using NewsCollector.Application.Enums;

namespace NewsCollector.Application.Abstractions;

public interface INewsQueryService
{
    Task<PagedResult<NewsItemListDto>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? sourceId = null,
        Guid? categoryId = null,
        bool? uncategorized = null,
        bool? hasContent = null,
        NewsToneFilter? toneFilter = null,
        Guid? editorialTagId = null,
        CancellationToken cancellationToken = default);

    Task<NewsItemDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RelatedNewsDto>> GetRelatedAsync(
        Guid newsId,
        CancellationToken cancellationToken = default);
}

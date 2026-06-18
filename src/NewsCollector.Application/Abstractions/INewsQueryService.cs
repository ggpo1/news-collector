using NewsCollector.Application.Dtos;

namespace NewsCollector.Application.Abstractions;

public interface INewsQueryService
{
    Task<PagedResult<NewsItemListDto>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? sourceId = null,
        bool? hasContent = null,
        CancellationToken cancellationToken = default);

    Task<NewsItemDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RelatedNewsDto>> GetRelatedAsync(
        Guid newsId,
        CancellationToken cancellationToken = default);
}

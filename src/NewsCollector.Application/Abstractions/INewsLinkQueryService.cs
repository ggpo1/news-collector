using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Application.Abstractions;

public interface INewsLinkQueryService
{
    Task<PagedResult<NewsLinkListDto>> GetPagedAsync(
        int page,
        int pageSize,
        LinkType? linkType = null,
        CancellationToken cancellationToken = default);
}

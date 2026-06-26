using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Application.Abstractions;

public interface ISearchQueryService
{
    Task<IReadOnlyList<SearchResultDto>> SearchAsync(
        string query,
        IReadOnlyList<SearchDocumentType>? types = null,
        int limit = 20,
        CancellationToken cancellationToken = default);
}

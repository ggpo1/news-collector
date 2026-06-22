using NewsCollector.Application.Dtos;

namespace NewsCollector.Application.Abstractions;

public interface ICategoryQueryService
{
    Task<IReadOnlyList<CategoryDto>> GetActiveAsync(CancellationToken cancellationToken = default);
}

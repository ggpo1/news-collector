using NewsCollector.Application.Dtos;

namespace NewsCollector.Application.Abstractions;

public interface ISourcesQueryService
{
    Task<IReadOnlyList<SourceDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<SourceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

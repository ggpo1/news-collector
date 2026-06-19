using NewsCollector.Application.Dtos;

namespace NewsCollector.Application.Abstractions;

public interface ISourcesService
{
    Task<IReadOnlyList<SourceDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<SourceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SourceDto?> CreateAsync(CreateSourceRequest request, CancellationToken cancellationToken = default);

    Task<SourceDto?> UpdateAsync(Guid id, UpdateSourceRequest request, CancellationToken cancellationToken = default);

    Task<SourceDeleteResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public enum SourceDeleteResult
{
    NotFound,
    HasNews,
    Deleted
}

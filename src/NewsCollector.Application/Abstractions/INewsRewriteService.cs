using NewsCollector.Application.Dtos;

namespace NewsCollector.Application.Abstractions;

public interface INewsRewriteService
{
    Task<PagedResult<NewsRewriteDto>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? sourceNewsId = null,
        CancellationToken cancellationToken = default);

    Task<NewsRewriteDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<MutationResult<NewsRewriteDto>> CreateAsync(
        CreateNewsRewriteRequest request,
        CancellationToken cancellationToken = default);

    Task<MutationResult<NewsRewriteDto>> UpdateAsync(
        Guid id,
        UpdateNewsRewriteRequest request,
        CancellationToken cancellationToken = default);

    Task<MutationResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

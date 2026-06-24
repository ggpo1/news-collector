using NewsCollector.Application.Dtos;

namespace NewsCollector.Application.Abstractions;

public interface INewsEditorialService
{
    Task<IReadOnlyList<EditorialTagDto>> GetTagsAsync(CancellationToken cancellationToken = default);

    Task<NewsItemDetailDto?> UpdateCategoryAsync(
        Guid newsId,
        Guid? categoryId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<NewsItemDetailDto?> UpdateEditorialTagsAsync(
        Guid newsId,
        IReadOnlyList<Guid> tagIds,
        Guid userId,
        CancellationToken cancellationToken = default);
}

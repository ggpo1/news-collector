using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Application.Abstractions;

public interface IStoryQueryService
{
    Task<PagedResult<StoryListDto>> GetPagedAsync(
        int page,
        int pageSize,
        StoryStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<StoryDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<StoryDetailDto?> GetByClusterKeyAsync(string clusterKey, CancellationToken cancellationToken = default);

    Task<Guid?> FindStoryIdForNewsAsync(Guid newsId, CancellationToken cancellationToken = default);
}

public interface IStoryCommandService
{
    Task<StoryDetailDto?> UpdateStatusAsync(
        Guid storyId,
        StoryStatus status,
        Guid userId,
        CancellationToken cancellationToken = default);
}

public interface IStorySyncService
{
    Task<int> SyncFromLinksAsync(CancellationToken cancellationToken = default);
}

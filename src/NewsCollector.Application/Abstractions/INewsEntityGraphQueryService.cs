using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Application.Abstractions;

public interface INewsEntityGraphQueryService
{
    Task<EntityGraphDto> GetCoMentionGraphAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        NamedEntityType? entityType = null,
        int minWeight = 2,
        int maxNodes = 150,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NamedEntityListDto>> SearchEntitiesAsync(
        string? query,
        NamedEntityType? entityType = null,
        int limit = 50,
        CancellationToken cancellationToken = default);

    Task<NamedEntityDetailDto?> GetEntityDetailAsync(
        Guid entityId,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken cancellationToken = default);
}

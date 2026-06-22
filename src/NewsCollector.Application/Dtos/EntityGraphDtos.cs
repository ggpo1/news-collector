using NewsCollector.Domain.Enums;

namespace NewsCollector.Application.Dtos;

public sealed record NamedEntityListDto(
    Guid Id,
    string Name,
    NamedEntityType Type,
    int MentionCount);

public sealed record NamedEntityDetailDto(
    Guid Id,
    string Name,
    NamedEntityType Type,
    int MentionCount,
    IReadOnlyList<NamedEntityNeighborDto> Neighbors);

public sealed record NamedEntityNeighborDto(
    Guid EntityId,
    string Name,
    NamedEntityType Type,
    int CoMentionCount);

public sealed record EntityGraphDto(
    DateTimeOffset From,
    DateTimeOffset To,
    IReadOnlyList<EntityGraphNodeDto> Nodes,
    IReadOnlyList<EntityGraphEdgeDto> Edges);

public sealed record EntityGraphNodeDto(
    Guid Id,
    string Name,
    NamedEntityType Type,
    int MentionCount);

public sealed record EntityGraphEdgeDto(
    Guid SourceId,
    Guid TargetId,
    int Weight);

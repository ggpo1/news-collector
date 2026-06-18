using NewsCollector.Domain.Enums;

namespace NewsCollector.Application.Dtos;

public sealed record SourceDto(
    Guid Id,
    string Name,
    SourceType Type,
    string Url,
    bool IsActive,
    int FetchIntervalMinutes,
    DateTimeOffset? LastFetchedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

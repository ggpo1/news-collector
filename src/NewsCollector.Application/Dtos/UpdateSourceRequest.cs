using NewsCollector.Domain.Enums;

namespace NewsCollector.Application.Dtos;

public sealed record UpdateSourceRequest(
    string Name,
    SourceType Type,
    string Url,
    bool IsActive,
    int FetchIntervalMinutes,
    bool ContentFetchEnabled = true,
    string? ContentSelector = null);

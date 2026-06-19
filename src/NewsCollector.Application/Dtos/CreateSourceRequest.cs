using NewsCollector.Domain.Enums;

namespace NewsCollector.Application.Dtos;

public sealed record CreateSourceRequest(
    string Name,
    SourceType Type,
    string Url,
    bool IsActive = true,
    int FetchIntervalMinutes = 15,
    bool ContentFetchEnabled = true,
    string? ContentSelector = null);

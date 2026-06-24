namespace NewsCollector.Application.Dtos;

public sealed record NewsItemListDto(
    Guid Id,
    Guid SourceId,
    string SourceName,
    string? CategoryName,
    decimal? ToneCoefficient,
    string Title,
    string? Summary,
    string Url,
    DateTimeOffset? PublishedAt,
    DateTimeOffset FetchedAt,
    bool HasContent,
    IReadOnlyList<EditorialTagDto> EditorialTags);

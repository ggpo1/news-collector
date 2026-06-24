namespace NewsCollector.Application.Dtos;

public sealed record NewsItemDetailDto(
    Guid Id,
    Guid SourceId,
    string SourceName,
    Guid? CategoryId,
    string? CategoryName,
    bool IsCategoryManual,
    decimal? ToneCoefficient,
    DateTimeOffset? ToneAnalyzedAt,
    string ExternalId,
    string Title,
    string? Summary,
    string? Content,
    string Url,
    DateTimeOffset? PublishedAt,
    DateTimeOffset FetchedAt,
    DateTimeOffset? ContentFetchedAt,
    string? ContentHash,
    DateTimeOffset CreatedAt,
    IReadOnlyList<EditorialTagDto> EditorialTags,
    Guid? StoryId);

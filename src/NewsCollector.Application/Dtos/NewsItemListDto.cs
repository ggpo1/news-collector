namespace NewsCollector.Application.Dtos;

public sealed record NewsItemListDto(
    Guid Id,
    Guid SourceId,
    string SourceName,
    string? CategoryName,
    string Title,
    string? Summary,
    string Url,
    DateTimeOffset? PublishedAt,
    DateTimeOffset FetchedAt,
    bool HasContent);

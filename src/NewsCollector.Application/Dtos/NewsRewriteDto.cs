namespace NewsCollector.Application.Dtos;

public sealed record NewsRewriteDto(
    Guid Id,
    Guid SourceNewsId,
    Guid SourceNewsSourceId,
    string SourceNewsSourceName,
    string SourceNewsTitle,
    string SourceNewsUrl,
    DateTimeOffset? SourceNewsPublishedAt,
    string Title,
    string? Summary,
    string? Content,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

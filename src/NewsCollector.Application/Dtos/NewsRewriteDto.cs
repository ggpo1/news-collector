namespace NewsCollector.Application.Dtos;

public sealed record NewsRewriteDto(
    Guid Id,
    Guid SourceNewsId,
    Guid SourceNewsSourceId,
    string SourceNewsSourceName,
    string SourceNewsTitle,
    string SourceNewsUrl,
    DateTimeOffset? SourceNewsPublishedAt,
    Guid AuthorId,
    string AuthorLogin,
    string AuthorDisplayName,
    string Title,
    string? Summary,
    string? Content,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

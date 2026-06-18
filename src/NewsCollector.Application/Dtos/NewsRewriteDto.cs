namespace NewsCollector.Application.Dtos;

public sealed record NewsRewriteDto(
    Guid Id,
    Guid SourceNewsId,
    string SourceNewsTitle,
    string Title,
    string? Summary,
    string? Content,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

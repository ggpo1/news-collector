namespace NewsCollector.Application.Dtos;

public sealed record CreateNewsRewriteRequest(
    Guid SourceNewsId,
    string Title,
    string? Summary,
    string? Content);

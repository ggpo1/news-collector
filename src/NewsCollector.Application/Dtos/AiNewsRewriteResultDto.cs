namespace NewsCollector.Application.Dtos;

public sealed record AiNewsRewriteResultDto(
    string Title,
    string? Summary,
    string Content);

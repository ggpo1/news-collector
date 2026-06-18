namespace NewsCollector.Application.Dtos;

public sealed record UpdateNewsRewriteRequest(
    string Title,
    string? Summary,
    string? Content);

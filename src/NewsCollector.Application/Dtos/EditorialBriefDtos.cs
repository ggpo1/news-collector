namespace NewsCollector.Application.Dtos;

public sealed record EditorialBriefReportDto(
    Guid Id,
    string Period,
    string Markdown,
    DateTimeOffset GeneratedAt,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    string? Model);

public sealed record EditorialBriefHistoryItemDto(
    Guid Id,
    string Period,
    DateTimeOffset GeneratedAt,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd);

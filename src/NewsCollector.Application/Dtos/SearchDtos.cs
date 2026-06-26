using NewsCollector.Domain.Enums;

namespace NewsCollector.Application.Dtos;

public sealed record SearchResultDto(
    SearchDocumentType DocumentType,
    Guid EntityId,
    string Title,
    string? Snippet,
    string? SourceName,
    DateTimeOffset? PublishedAt,
    double Score);

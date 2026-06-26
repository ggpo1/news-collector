using NewsCollector.Domain.Enums;

namespace NewsCollector.Domain.Entities;

public class SearchDocument
{
    public Guid Id { get; set; }

    public SearchDocumentType DocumentType { get; set; }

    public Guid EntityId { get; set; }

    public ContentLanguage Language { get; set; }

    public required string Title { get; set; }

    public string? Body { get; set; }

    public string? SourceName { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

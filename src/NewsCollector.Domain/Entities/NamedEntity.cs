using NewsCollector.Domain.Enums;

namespace NewsCollector.Domain.Entities;

public class NamedEntity
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public NamedEntityType Type { get; set; }

    /// <summary>
    /// Stable key for deduplication, e.g. "person:vladimir-putin".
    /// </summary>
    public required string NormalizedKey { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<NewsEntityMention> Mentions { get; set; } = [];
}

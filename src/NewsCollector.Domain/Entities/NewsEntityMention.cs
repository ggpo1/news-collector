namespace NewsCollector.Domain.Entities;

public class NewsEntityMention
{
    public Guid Id { get; set; }

    public Guid NewsItemId { get; set; }

    public Guid NamedEntityId { get; set; }

    /// <summary>
    /// Surface form as found in the news text.
    /// </summary>
    public required string MentionText { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public NewsItem NewsItem { get; set; } = null!;

    public NamedEntity NamedEntity { get; set; } = null!;
}

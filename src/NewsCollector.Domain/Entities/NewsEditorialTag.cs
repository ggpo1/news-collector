namespace NewsCollector.Domain.Entities;

public class NewsEditorialTag
{
    public Guid NewsItemId { get; set; }

    public Guid EditorialTagId { get; set; }

    public Guid? AddedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public NewsItem NewsItem { get; set; } = null!;

    public EditorialTag EditorialTag { get; set; } = null!;

    public User? AddedBy { get; set; }
}

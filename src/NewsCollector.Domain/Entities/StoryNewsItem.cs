namespace NewsCollector.Domain.Entities;

public class StoryNewsItem
{
    public Guid StoryId { get; set; }

    public Guid NewsItemId { get; set; }

    public DateTimeOffset LinkedAt { get; set; }

    public Story Story { get; set; } = null!;

    public NewsItem NewsItem { get; set; } = null!;
}

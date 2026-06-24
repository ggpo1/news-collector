namespace NewsCollector.Domain.Entities;

public class NewsItemEmbedding
{
    public Guid NewsItemId { get; set; }

    public required string Model { get; set; }

    public required float[] Vector { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public NewsItem NewsItem { get; set; } = null!;
}

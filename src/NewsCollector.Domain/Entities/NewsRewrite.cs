namespace NewsCollector.Domain.Entities;

public class NewsRewrite
{
    public Guid Id { get; set; }

    public Guid SourceNewsId { get; set; }

    public required string Title { get; set; }

    public string? Summary { get; set; }

    public string? Content { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public NewsItem SourceNews { get; set; } = null!;
}

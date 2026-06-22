namespace NewsCollector.Domain.Entities;

public class NewsItem
{
    public Guid Id { get; set; }

    public Guid SourceId { get; set; }

    public Guid? CategoryId { get; set; }

    public required string ExternalId { get; set; }

    public required string Title { get; set; }

    public string? Summary { get; set; }

    public string? Content { get; set; }

    public required string Url { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }

    public DateTimeOffset FetchedAt { get; set; }

    public DateTimeOffset? ContentFetchedAt { get; set; }

    public string? ContentHash { get; set; }

    public string? RawPayload { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Sentiment tone from -1 (negative) to +1 (positive). Null until analyzed.
    /// </summary>
    public decimal? ToneCoefficient { get; set; }

    public DateTimeOffset? ToneAnalyzedAt { get; set; }

    public Source Source { get; set; } = null!;

    public Category? Category { get; set; }

    public ICollection<NewsLink> LinksAsLow { get; set; } = [];

    public ICollection<NewsLink> LinksAsHigh { get; set; } = [];

    public ICollection<NewsRewrite> Rewrites { get; set; } = [];
}

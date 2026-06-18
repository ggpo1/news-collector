using NewsCollector.Domain.Enums;

namespace NewsCollector.Domain.Entities;

public class Source
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public SourceType Type { get; set; }

    public required string Url { get; set; }

    public bool IsActive { get; set; } = true;

    public int FetchIntervalMinutes { get; set; } = 15;

    public DateTimeOffset? LastFetchedAt { get; set; }

    public string? Config { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<NewsItem> NewsItems { get; set; } = [];
}

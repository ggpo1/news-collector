using NewsCollector.Domain.Enums;

namespace NewsCollector.Domain.Entities;

public class Story
{
    public Guid Id { get; set; }

    /// <summary>Stable cluster identifier (min news id in the cluster).</summary>
    public required string ClusterKey { get; set; }

    public required string Title { get; set; }

    public StoryStatus Status { get; set; } = StoryStatus.Monitoring;

    public Guid? PrimaryNewsItemId { get; set; }

    public int ArticleCount { get; set; }

    public int SourceCount { get; set; }

    public DateTimeOffset? FirstSeenAt { get; set; }

    public DateTimeOffset? LastActivityAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Guid? StatusUpdatedByUserId { get; set; }

    public DateTimeOffset? StatusUpdatedAt { get; set; }

    public User? StatusUpdatedBy { get; set; }

    public NewsItem? PrimaryNewsItem { get; set; }

    public ICollection<StoryNewsItem> NewsItems { get; set; } = [];
}

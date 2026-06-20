namespace NewsCollector.Domain.Entities;

public class Category
{
    public Guid Id { get; set; }

    public required string Slug { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<NewsItem> NewsItems { get; set; } = [];
}

namespace NewsCollector.Domain.Entities;

public class EditorialTag
{
    public Guid Id { get; set; }

    public required string Slug { get; set; }

    public required string Name { get; set; }

    public string? Color { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<NewsEditorialTag> NewsItems { get; set; } = [];
}

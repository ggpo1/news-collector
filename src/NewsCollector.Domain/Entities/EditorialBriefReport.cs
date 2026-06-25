using NewsCollector.Domain.Enums;

namespace NewsCollector.Domain.Entities;

public class EditorialBriefReport
{
    public Guid Id { get; set; }

    public EditorialBriefPeriod Period { get; set; }

    public required string Markdown { get; set; }

    public DateTimeOffset GeneratedAt { get; set; }

    public DateTimeOffset WindowStart { get; set; }

    public DateTimeOffset WindowEnd { get; set; }

    public string? Model { get; set; }
}

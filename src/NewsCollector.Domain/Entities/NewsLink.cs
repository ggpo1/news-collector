using NewsCollector.Domain.Enums;

namespace NewsCollector.Domain.Entities;

public class NewsLink
{
    public Guid Id { get; set; }

    public Guid NewsIdLow { get; set; }

    public Guid NewsIdHigh { get; set; }

    public LinkType LinkType { get; set; }

    public LinkMethod LinkMethod { get; set; }

    public decimal Confidence { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public NewsItem NewsLow { get; set; } = null!;

    public NewsItem NewsHigh { get; set; } = null!;
}

using NewsCollector.Domain.Enums;

namespace NewsCollector.Domain.Entities;

public class TelegramDelivery
{
    public Guid Id { get; set; }

    public Guid TelegramChannelId { get; set; }

    public TelegramChannel Channel { get; set; } = null!;

    public Guid? NewsItemId { get; set; }

    public NewsItem? NewsItem { get; set; }

    public Guid? NewsRewriteId { get; set; }

    public NewsRewrite? NewsRewrite { get; set; }

    public required string MessageText { get; set; }

    public TelegramDeliveryStatus Status { get; set; } = TelegramDeliveryStatus.Pending;

    public string? ErrorMessage { get; set; }

    public long? TelegramMessageId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? SentAt { get; set; }
}

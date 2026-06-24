namespace NewsCollector.Domain.Entities;

public class TelegramChannel
{
    public Guid Id { get; set; }

    public Guid TelegramBotId { get; set; }

    public TelegramBot Bot { get; set; } = null!;

    public required string Name { get; set; }

    /// <summary>Telegram chat id, e.g. -1001234567890 or @channelusername.</summary>
    public required string ChatId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<TelegramChannelCategory> ChannelCategories { get; set; } = [];

    public ICollection<TelegramChannelSource> ChannelSources { get; set; } = [];
}

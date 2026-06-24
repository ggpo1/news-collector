namespace NewsCollector.Domain.Entities;

public class TelegramChannelCategory
{
    public Guid TelegramChannelId { get; set; }

    public TelegramChannel Channel { get; set; } = null!;

    public Guid CategoryId { get; set; }

    public Category Category { get; set; } = null!;
}

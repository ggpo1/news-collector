namespace NewsCollector.Domain.Entities;

public class TelegramChannelSource
{
    public Guid TelegramChannelId { get; set; }

    public TelegramChannel Channel { get; set; } = null!;

    public Guid SourceId { get; set; }

    public Source Source { get; set; } = null!;
}

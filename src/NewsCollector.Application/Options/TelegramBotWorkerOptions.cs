namespace NewsCollector.Application.Options;

public sealed class TelegramBotWorkerOptions
{
    public const string SectionName = "TelegramBot";

    public Guid? BotId { get; set; }

    public int PollingIntervalSeconds { get; set; } = 5;

    public int BatchSize { get; set; } = 10;
}

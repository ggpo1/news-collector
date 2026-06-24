namespace NewsCollector.Application.Options;

public sealed class TelegramBotOrchestratorOptions
{
    public const string SectionName = "TelegramBotOrchestrator";

    public bool Enabled { get; set; } = true;

    public string DockerImage { get; set; } = "news-collector-telegram-bot:latest";

    public string DockerNetwork { get; set; } = "news-collector_default";

    public string WorkerConnectionString { get; set; } = string.Empty;
}

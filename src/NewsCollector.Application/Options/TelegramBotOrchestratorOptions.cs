namespace NewsCollector.Application.Options;

public sealed class TelegramBotOrchestratorOptions
{
    public const string SectionName = "TelegramBotOrchestrator";

    public bool Enabled { get; set; } = true;

    public string DockerImage { get; set; } = "news-collector-telegram-bot:latest";

    public string DockerNetwork { get; set; } = "news-collector_default";

    public string WorkerConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// HTTP proxy for Telegram bot worker containers (e.g. http://host.docker.internal:10809).
    /// .NET HttpClient supports HTTP proxy; SOCKS5 requires HTTP inbound on Xray instead.
    /// </summary>
    public string? WorkerHttpProxy { get; set; }

    public string? WorkerHttpsProxy { get; set; }
}

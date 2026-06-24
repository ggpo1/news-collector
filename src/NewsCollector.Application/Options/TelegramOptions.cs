namespace NewsCollector.Application.Options;

public sealed class TelegramOptions
{
    public const string SectionName = "Telegram";

    /// <summary>
    /// HTTP proxy URL for Telegram Bot API (e.g. http://host.docker.internal:10809).
    /// Falls back to HTTPS_PROXY / HTTP_PROXY environment variables.
    /// </summary>
    public string? HttpProxy { get; set; }

    public string? HttpsProxy { get; set; }

    /// <summary>
    /// Comma-separated hosts bypassing proxy (default includes postgres and localhost).
    /// </summary>
    public string? NoProxy { get; set; }
}

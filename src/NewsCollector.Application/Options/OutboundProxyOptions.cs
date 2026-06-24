namespace NewsCollector.Application.Options;

public sealed class OutboundProxyOptions
{
    public const string SectionName = "OutboundProxy";

    /// <summary>
    /// HTTP proxy for outbound requests (e.g. http://host.docker.internal:10809).
    /// </summary>
    public string? HttpProxy { get; set; }

    public string? HttpsProxy { get; set; }

    /// <summary>
    /// Comma-separated hosts that bypass the proxy (postgres, internal services).
    /// </summary>
    public string? NoProxy { get; set; }
}

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using NewsCollector.Application.Options;

namespace NewsCollector.Infrastructure.Telegram;

public sealed class TelegramProxyDiagnosticsService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public TelegramProxyDiagnosticsService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<TelegramProxyDiagnosticsResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var options = _configuration.GetSection(TelegramOptions.SectionName).Get<TelegramOptions>() ?? new TelegramOptions();
        var proxyUrl = FirstNonEmpty(
            options.HttpsProxy,
            options.HttpProxy,
            _configuration["HTTPS_PROXY"],
            _configuration["HTTP_PROXY"]);

        var result = new TelegramProxyDiagnosticsResult { ProxyUrl = proxyUrl };

        if (string.IsNullOrWhiteSpace(proxyUrl))
        {
            result.Summary = "Telegram proxy is not configured.";
            return result;
        }

        if (!Uri.TryCreate(proxyUrl, UriKind.Absolute, out var proxyUri))
        {
            result.Summary = $"Invalid proxy URL: {proxyUrl}";
            return result;
        }

        result.ProxyHost = proxyUri.Host;
        result.ProxyPort = proxyUri.Port;

        try
        {
            var addresses = await Dns.GetHostAddressesAsync(proxyUri.Host);
            result.ProxyHostAddresses = addresses.Select(address => address.ToString()).ToArray();
        }
        catch (Exception ex)
        {
            result.ProxyDnsError = ex.Message;
        }

        var tcpStopwatch = Stopwatch.StartNew();
        try
        {
            using var tcp = new TcpClient();
            var connectTask = tcp.ConnectAsync(proxyUri.Host, proxyUri.Port);
            var completed = await Task.WhenAny(connectTask, Task.Delay(TimeSpan.FromSeconds(10), cancellationToken));
            if (completed != connectTask)
            {
                throw new TimeoutException($"TCP connect to {proxyUri.Host}:{proxyUri.Port} timed out after 10s");
            }

            await connectTask;
            result.ProxyTcpReachable = true;
            result.ProxyTcpLatencyMs = tcpStopwatch.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            result.ProxyTcpReachable = false;
            result.ProxyTcpError = ex.Message;
        }

        var httpStopwatch = Stopwatch.StartNew();
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(20));

            var client = _httpClientFactory.CreateClient("Telegram");
            using var response = await client.GetAsync("https://api.telegram.org/", cts.Token);
            result.TelegramApiReachable = true;
            result.TelegramHttpStatusCode = (int)response.StatusCode;
            result.TelegramApiLatencyMs = httpStopwatch.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            result.TelegramApiReachable = false;
            result.TelegramApiError = ex.Message;
        }

        result.Summary = result.TelegramApiReachable
            ? "Telegram API reachable through proxy."
            : result.ProxyTcpReachable
                ? "Proxy TCP OK, but Telegram API request failed (check Xray outbound/routing)."
                : "Cannot connect to proxy TCP port (Xray listen 0.0.0.0? ufw allow from Docker subnet?).";

        return result;
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }
}

public sealed class TelegramProxyDiagnosticsResult
{
    public string? ProxyUrl { get; init; }

    public string? ProxyHost { get; set; }

    public int ProxyPort { get; set; }

    public string[] ProxyHostAddresses { get; set; } = [];

    public string? ProxyDnsError { get; set; }

    public bool ProxyTcpReachable { get; set; }

    public long? ProxyTcpLatencyMs { get; set; }

    public string? ProxyTcpError { get; set; }

    public bool TelegramApiReachable { get; set; }

    public int? TelegramHttpStatusCode { get; set; }

    public long? TelegramApiLatencyMs { get; set; }

    public string? TelegramApiError { get; set; }

    public string Summary { get; set; } = string.Empty;
}

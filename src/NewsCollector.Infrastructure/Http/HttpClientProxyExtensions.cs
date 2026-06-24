using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NewsCollector.Application.Options;

namespace NewsCollector.Infrastructure.Http;

public static class HttpClientProxyExtensions
{
    public static IHttpClientBuilder ConfigureOutboundProxy(
        this IHttpClientBuilder builder,
        IConfiguration configuration)
    {
        var outbound = configuration.GetSection(OutboundProxyOptions.SectionName).Get<OutboundProxyOptions>()
            ?? new OutboundProxyOptions();
        var telegram = configuration.GetSection(TelegramOptions.SectionName).Get<TelegramOptions>()
            ?? new TelegramOptions();

        var httpsProxy = FirstNonEmpty(
            outbound.HttpsProxy,
            outbound.HttpProxy,
            telegram.HttpsProxy,
            telegram.HttpProxy,
            configuration["HTTPS_PROXY"],
            configuration["HTTP_PROXY"]);

        if (!string.IsNullOrWhiteSpace(httpsProxy)
            && (httpsProxy.StartsWith("socks5://", StringComparison.OrdinalIgnoreCase)
                || httpsProxy.StartsWith("socks4://", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "Outbound proxy must be an HTTP proxy URL (e.g. http://host.docker.internal:10809). " +
                ".NET HttpClient does not support SOCKS5 without extra packages. " +
                "Use Xray HTTP inbound on port 10809.");
        }

        var noProxy = FirstNonEmpty(
            outbound.NoProxy,
            telegram.NoProxy,
            configuration["NO_PROXY"],
            "postgres,localhost,127.0.0.1,::1,ollama") ?? "postgres,localhost,127.0.0.1,::1,ollama";

        var bypassList = noProxy
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(host => host.Trim())
            .Where(host => host.Length > 0)
            .ToArray();

        return builder.ConfigurePrimaryHttpMessageHandler(() => CreateHandler(httpsProxy, bypassList));
    }

    internal static SocketsHttpHandler CreateHandler(string? httpsProxy, string[]? bypassList = null)
    {
        var handler = new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            ConnectTimeout = TimeSpan.FromSeconds(15)
        };

        if (string.IsNullOrWhiteSpace(httpsProxy))
        {
            return handler;
        }

        handler.Proxy = new WebProxy(new Uri(httpsProxy), false)
        {
            BypassList = bypassList ?? []
        };
        handler.UseProxy = true;
        return handler;
    }

    public static string? ResolveProxyUrl(IConfiguration configuration)
    {
        var outbound = configuration.GetSection(OutboundProxyOptions.SectionName).Get<OutboundProxyOptions>()
            ?? new OutboundProxyOptions();
        var telegram = configuration.GetSection(TelegramOptions.SectionName).Get<TelegramOptions>()
            ?? new TelegramOptions();

        return FirstNonEmpty(
            outbound.HttpsProxy,
            outbound.HttpProxy,
            telegram.HttpsProxy,
            telegram.HttpProxy,
            configuration["HTTPS_PROXY"],
            configuration["HTTP_PROXY"]);
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
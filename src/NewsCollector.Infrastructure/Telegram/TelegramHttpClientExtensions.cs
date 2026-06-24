using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NewsCollector.Application.Options;

namespace NewsCollector.Infrastructure.Telegram;

internal static class TelegramHttpClientExtensions
{
    public static IHttpClientBuilder ConfigureTelegramProxy(
        this IHttpClientBuilder builder,
        IConfiguration configuration)
    {
        var options = configuration.GetSection(TelegramOptions.SectionName).Get<TelegramOptions>() ?? new TelegramOptions();

        var httpsProxy = FirstNonEmpty(
            options.HttpsProxy,
            options.HttpProxy,
            configuration["HTTPS_PROXY"],
            configuration["HTTP_PROXY"]);

        if (string.IsNullOrWhiteSpace(httpsProxy))
        {
            return builder;
        }

        if (httpsProxy.StartsWith("socks5://", StringComparison.OrdinalIgnoreCase)
            || httpsProxy.StartsWith("socks4://", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Telegram proxy must be an HTTP proxy URL (e.g. http://host.docker.internal:10809). " +
                ".NET HttpClient does not support SOCKS5 without extra packages. " +
                "Use Xray HTTP inbound on port 10809.");
        }

        var noProxy = FirstNonEmpty(
            options.NoProxy,
            configuration["NO_PROXY"],
            "postgres,localhost,127.0.0.1,::1") ?? "postgres,localhost,127.0.0.1,::1";

        var bypassList = noProxy
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(host => host.Trim())
            .Where(host => host.Length > 0)
            .ToArray();

        return builder.ConfigurePrimaryHttpMessageHandler(() =>
        {
            var proxy = new WebProxy(new Uri(httpsProxy), false)
            {
                BypassList = bypassList
            };

            return new SocketsHttpHandler
            {
                Proxy = proxy,
                UseProxy = true
            };
        });
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

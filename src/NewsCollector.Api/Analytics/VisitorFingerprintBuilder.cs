using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace NewsCollector.Api.Analytics;

public static class VisitorFingerprintBuilder
{
    public const string VisitorIdHeaderName = "X-Visitor-Id";

    public static string Build(HttpContext context)
    {
        var clientId = context.Request.Headers[VisitorIdHeaderName].FirstOrDefault()?.Trim();
        if (!string.IsNullOrWhiteSpace(clientId))
        {
            return Hash($"cid:{clientId}");
        }

        var ip = GetClientIp(context);
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var acceptLanguage = context.Request.Headers.AcceptLanguage.ToString();

        return Hash($"ip:{ip}|ua:{userAgent}|lang:{acceptLanguage}");
    }

    public static string? GetTruncatedUserAgent(HttpContext context)
    {
        var userAgent = context.Request.Headers.UserAgent.ToString().Trim();
        if (userAgent.Length == 0)
        {
            return null;
        }

        return userAgent.Length <= 512 ? userAgent : userAgent[..512];
    }

    private static string GetClientIp(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            var firstIp = forwardedFor.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(firstIp))
            {
                return firstIp;
            }
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(realIp))
        {
            return realIp.Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static string Hash(string material)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

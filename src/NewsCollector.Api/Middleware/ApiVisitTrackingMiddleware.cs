using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Models;
using NewsCollector.Api.Analytics;

namespace NewsCollector.Api.Middleware;

public sealed class ApiVisitTrackingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiVisitTrackingMiddleware> _logger;

    public ApiVisitTrackingMiddleware(RequestDelegate next, ILogger<ApiVisitTrackingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IServiceScopeFactory scopeFactory)
    {
        if (!ShouldTrack(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var requestedAt = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        try
        {
            using var scope = scopeFactory.CreateScope();
            var writer = scope.ServiceProvider.GetRequiredService<IApiVisitWriter>();

            var queryString = context.Request.QueryString.HasValue
                ? Truncate(context.Request.QueryString.Value, 2048)
                : null;

            await writer.LogAsync(
                new ApiVisitEntry(
                    requestedAt,
                    context.Request.Method,
                    context.Request.Path.Value ?? "/",
                    queryString,
                    context.Response.StatusCode,
                    (int)stopwatch.ElapsedMilliseconds,
                    VisitorFingerprintBuilder.Build(context),
                    VisitorFingerprintBuilder.GetTruncatedUserAgent(context)),
                context.RequestAborted);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist API visit for {Path}", context.Request.Path);
        }
    }

    private static bool ShouldTrack(PathString path)
    {
        var value = path.Value;
        return value is not null
            && value.StartsWith("/api/", StringComparison.OrdinalIgnoreCase);
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}

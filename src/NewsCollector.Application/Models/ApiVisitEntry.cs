namespace NewsCollector.Application.Models;

public sealed record ApiVisitEntry(
    DateTimeOffset RequestedAt,
    string HttpMethod,
    string Path,
    string? QueryString,
    int StatusCode,
    int DurationMs,
    string VisitorFingerprint,
    string? UserAgent,
    Guid? UserId);

namespace NewsCollector.Application.Models;

public sealed record ParsedFeedItem(
    string ExternalId,
    string Title,
    string? Summary,
    string Url,
    DateTimeOffset? PublishedAt,
    string RawPayload);

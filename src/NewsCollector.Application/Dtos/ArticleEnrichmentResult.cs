namespace NewsCollector.Application.Dtos;

public sealed record ArticleEnrichmentResult(
    string Status,
    NewsItemDetailDto? Item,
    string? Message)
{
    public const string StatusEnriched = "enriched";
    public const string StatusAlreadyEnriched = "already_enriched";
    public const string StatusNotSupported = "not_supported";
    public const string StatusFailed = "failed";
    public const string StatusNotFound = "not_found";
}

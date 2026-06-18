namespace NewsCollector.Application.Models;

public sealed class SourceScrapingConfig
{
    public bool ContentFetchEnabled { get; set; } = true;

    public string? ContentSelector { get; set; }
}

namespace NewsCollector.Application.Options;

public sealed class NewsSearchIndexerOptions
{
    public const string SectionName = "NewsSearchIndexer";

    public int PollingIntervalSeconds { get; set; } = 30;

    public int BatchSize { get; set; } = 25;

    public int DelayBetweenItemsMs { get; set; } = 0;

    /// <summary>Max characters from article body included in the search index.</summary>
    public int MaxBodyCharacters { get; set; } = 8000;
}

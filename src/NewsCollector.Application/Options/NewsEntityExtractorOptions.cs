namespace NewsCollector.Application.Options;

public sealed class NewsEntityExtractorOptions
{
    public const string SectionName = "NewsEntityExtractor";

    public int PollingIntervalSeconds { get; set; } = 60;

    public int BatchSize { get; set; } = 5;

    public int DelayBetweenItemsMs { get; set; } = 500;

    public int MaxEntitiesPerNews { get; set; } = 20;
}

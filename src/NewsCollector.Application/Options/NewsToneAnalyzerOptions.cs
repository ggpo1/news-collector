namespace NewsCollector.Application.Options;

public sealed class NewsToneAnalyzerOptions
{
    public const string SectionName = "NewsToneAnalyzer";

    public int PollingIntervalSeconds { get; set; } = 60;

    public int BatchSize { get; set; } = 10;

    public int DelayBetweenItemsMs { get; set; } = 200;
}

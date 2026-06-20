namespace NewsCollector.Application.Options;

public sealed class NewsCategorizerOptions
{
    public const string SectionName = "NewsCategorizer";

    public int PollingIntervalSeconds { get; set; } = 60;

    public int BatchSize { get; set; } = 10;

    public int DelayBetweenItemsMs { get; set; } = 200;
}

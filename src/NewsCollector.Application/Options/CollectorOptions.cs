namespace NewsCollector.Application.Options;

public sealed class CollectorOptions
{
    public const string SectionName = "Collector";

    public int PollingIntervalSeconds { get; set; } = 60;

    public int ContentEnrichmentBatchSize { get; set; } = 10;

    public int ContentEnrichmentDelayMs { get; set; } = 500;
}

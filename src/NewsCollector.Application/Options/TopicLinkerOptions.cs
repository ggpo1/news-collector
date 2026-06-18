namespace NewsCollector.Application.Options;

public sealed class TopicLinkerOptions
{
    public const string SectionName = "TopicLinker";

    public int PollingIntervalSeconds { get; set; } = 60;

    /// <summary>How far back to search for comparable news items.</summary>
    public int LookbackHours { get; set; } = 168;

    /// <summary>Maximum number of recent news items loaded per cycle.</summary>
    public int MaxCandidates { get; set; } = 300;

    /// <summary>Minimum Jaccard similarity (0–1) to create a SameTopic link.</summary>
    public decimal MinSimilarity { get; set; } = 0.45m;

    /// <summary>Minimum shared significant tokens required in addition to MinSimilarity.</summary>
    public int MinSharedTokens { get; set; } = 2;

    /// <summary>Similarity at or above which the link is classified as Duplicate.</summary>
    public decimal DuplicateSimilarity { get; set; } = 0.92m;
}

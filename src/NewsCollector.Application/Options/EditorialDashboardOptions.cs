namespace NewsCollector.Application.Options;

public sealed class EditorialDashboardOptions
{
    public const string SectionName = "EditorialDashboard";

    public int WindowHours { get; set; } = 48;

    public int MinSourcesForDevelopingTopic { get; set; } = 3;

    public int MinArticlesForDuplicateGroup { get; set; } = 2;

    public int MaxDevelopingTopics { get; set; } = 15;

    public int MaxDuplicateGroups { get; set; } = 15;

    public int MaxEntitySpikes { get; set; } = 12;

    public int MaxToneHighlights { get; set; } = 10;

    public int EntitySpikeMinMentions { get; set; } = 4;

    public double EntitySpikeMinRatio { get; set; } = 1.8;

    public decimal MinToneAbsolute { get; set; } = 0.55m;
}

namespace NewsCollector.Application.Options;

public sealed class EditorialBriefOptions
{
    public const string SectionName = "EditorialBrief";

    /// <summary>How often the worker checks whether a scheduled brief is due.</summary>
    public int PollingIntervalSeconds { get; set; } = 3600;

    /// <summary>UTC hours when morning/evening briefs are generated (even index = morning slot, odd = evening).</summary>
    public int[] ScheduleHoursUtc { get; set; } = [6, 18];

    /// <summary>Lookback window for aggregating topics, deliveries, and gaps.</summary>
    public int WindowHours { get; set; } = 12;

    public int MaxTopics { get; set; } = 7;

    public int MaxEntitySpikes { get; set; } = 5;

    public int MaxTelegramItems { get; set; } = 20;

    public string[] RuSourcePatterns { get; set; } =
    [
        ".ru",
        "ria.ru",
        "tass.ru",
        "rbc.ru",
        "kommersant",
        "vedomosti",
        "interfax",
        "lenta.ru",
        "gazeta.ru",
        "mk.ru",
        "iz.ru"
    ];

    public string[] WesternSourcePatterns { get; set; } =
    [
        "theguardian",
        "reuters",
        "bbc.",
        "bbc.com",
        "nytimes",
        "washingtonpost",
        "apnews",
        "cnn.com",
        "ft.com",
        "economist",
        "politico",
        "bloomberg",
        "wsj.com"
    ];
}

namespace NewsCollector.Application.Dtos;

public sealed record EditorialDashboardDto(
    IReadOnlyList<DevelopingTopicDto> DevelopingTopics,
    IReadOnlyList<DuplicateGroupDto> DuplicateGroups,
    IReadOnlyList<EntitySpikeDto> EntitySpikes,
    IReadOnlyList<ToneHighlightDto> ToneHighlights,
    EditorialDashboardMetaDto Meta);

public sealed record EditorialDashboardMetaDto(
    int WindowHours,
    DateTimeOffset WindowStart,
    DateTimeOffset GeneratedAt,
    int NewsInWindow);

public sealed record DevelopingTopicDto(
    string ClusterKey,
    string Headline,
    int SourceCount,
    int ArticleCount,
    IReadOnlyList<string> SourceNames,
    EditorialBriefNewsDto PrimaryArticle,
    IReadOnlyList<EditorialBriefNewsDto> RelatedArticles);

public sealed record DuplicateGroupDto(
    string ClusterKey,
    string Headline,
    int SourceCount,
    int ArticleCount,
    bool HasDuplicateLink,
    IReadOnlyList<string> SourceNames,
    EditorialBriefNewsDto PrimaryArticle,
    IReadOnlyList<EditorialBriefNewsDto> RelatedArticles);

public sealed record EntitySpikeDto(
    Guid EntityId,
    string EntityName,
    string EntityType,
    int MentionsInWindow,
    int MentionsInPreviousWindow,
    double SpikeRatio,
    IReadOnlyList<EditorialBriefNewsDto> RecentArticles);

public sealed record ToneHighlightDto(
    EditorialBriefNewsDto News,
    decimal ToneCoefficient,
    string ToneLabel);

public sealed record EditorialBriefNewsDto(
    Guid Id,
    string Title,
    string SourceName,
    string? CategoryName,
    decimal? ToneCoefficient,
    DateTimeOffset? PublishedAt,
    DateTimeOffset FetchedAt,
    bool HasContent,
    string Url);

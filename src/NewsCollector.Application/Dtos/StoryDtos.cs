using NewsCollector.Domain.Enums;

namespace NewsCollector.Application.Dtos;

public sealed record StoryListDto(
    Guid Id,
    string ClusterKey,
    string Title,
    StoryStatus Status,
    int ArticleCount,
    int SourceCount,
    DateTimeOffset? FirstSeenAt,
    DateTimeOffset? LastActivityAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<string> SourceNames);

public sealed record StoryDetailDto(
    Guid Id,
    string ClusterKey,
    string Title,
    StoryStatus Status,
    int ArticleCount,
    int SourceCount,
    DateTimeOffset? FirstSeenAt,
    DateTimeOffset? LastActivityAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid? PrimaryNewsItemId,
    IReadOnlyList<StoryTimelineItemDto> Timeline,
    IReadOnlyList<StoryEntityDto> KeyEntities,
    IReadOnlyList<StoryRewriteDto> Rewrites,
    IReadOnlyList<StoryTelegramDeliveryDto> TelegramDeliveries);

public sealed record StoryTimelineItemDto(
    Guid Id,
    string Title,
    string SourceName,
    string? CategoryName,
    decimal? ToneCoefficient,
    DateTimeOffset? PublishedAt,
    DateTimeOffset FetchedAt,
    bool HasContent,
    string Url,
    bool IsPrimary);

public sealed record StoryEntityDto(
    Guid Id,
    string Name,
    string Type,
    int MentionCount);

public sealed record StoryRewriteDto(
    Guid Id,
    Guid SourceNewsId,
    string SourceNewsTitle,
    string Title,
    string AuthorName,
    DateTimeOffset UpdatedAt);

public sealed record StoryTelegramDeliveryDto(
    Guid Id,
    string ChannelName,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SentAt,
    Guid? NewsItemId,
    Guid? NewsRewriteId);

public sealed record UpdateStoryStatusRequest(StoryStatus Status);

public sealed record EditorialTagDto(
    Guid Id,
    string Slug,
    string Name,
    string? Color);

public sealed record UpdateNewsCategoryRequest(Guid? CategoryId);

public sealed record UpdateNewsEditorialTagsRequest(IReadOnlyList<Guid> TagIds);

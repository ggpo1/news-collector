namespace NewsCollector.Application.Dtos;

public sealed record TelegramBotDto(
    Guid Id,
    string Name,
    string BotTokenMasked,
    bool IsActive,
    string ContainerStatus,
    string? ContainerName,
    string? ContainerError,
    int ChannelCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateTelegramBotRequest(string Name, string BotToken, bool IsActive);

public sealed record UpdateTelegramBotRequest(string Name, string? BotToken, bool IsActive);

public sealed record TelegramChannelDto(
    Guid Id,
    Guid TelegramBotId,
    string BotName,
    string Name,
    string ChatId,
    bool IsActive,
    IReadOnlyList<Guid> CategoryIds,
    IReadOnlyList<Guid> SourceIds,
    IReadOnlyList<string> CategoryNames,
    IReadOnlyList<string> SourceNames,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateTelegramChannelRequest(
    Guid TelegramBotId,
    string Name,
    string ChatId,
    bool IsActive,
    IReadOnlyList<Guid> CategoryIds,
    IReadOnlyList<Guid> SourceIds);

public sealed record UpdateTelegramChannelRequest(
    Guid TelegramBotId,
    string Name,
    string ChatId,
    bool IsActive,
    IReadOnlyList<Guid> CategoryIds,
    IReadOnlyList<Guid> SourceIds);

public sealed record TelegramSendRequest(Guid ChannelId);

public sealed record TelegramSendResultDto(
    Guid DeliveryId,
    string Status,
    string Message);

public sealed record TelegramDeliveryDto(
    Guid Id,
    Guid ChannelId,
    string ChannelName,
    string Status,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SentAt);

using NewsCollector.Domain.Enums;

namespace NewsCollector.Domain.Entities;

public class TelegramBot
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string BotToken { get; set; }

    public bool IsActive { get; set; } = true;

    public string? ContainerName { get; set; }

    public TelegramBotContainerStatus ContainerStatus { get; set; } = TelegramBotContainerStatus.Stopped;

    public string? ContainerError { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<TelegramChannel> Channels { get; set; } = [];
}

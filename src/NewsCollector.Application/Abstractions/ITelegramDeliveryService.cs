using NewsCollector.Application.Dtos;

namespace NewsCollector.Application.Abstractions;

public interface ITelegramDeliveryService
{
    Task<TelegramSendResultDto?> QueueNewsAsync(Guid newsId, Guid channelId, CancellationToken cancellationToken = default);

    Task<TelegramSendResultDto?> QueueRewriteAsync(Guid rewriteId, Guid channelId, CancellationToken cancellationToken = default);

    Task<int> ProcessPendingForBotAsync(Guid botId, CancellationToken cancellationToken = default);

    Task<TelegramDeliveryDto?> GetDeliveryAsync(Guid deliveryId, CancellationToken cancellationToken = default);
}

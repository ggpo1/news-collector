using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Application.Options;
using NewsCollector.Domain.Entities;
using NewsCollector.Domain.Enums;
using NewsCollector.Infrastructure.Persistence;
using NewsCollector.Infrastructure.Telegram;

namespace NewsCollector.Infrastructure.Services;

public sealed class TelegramDeliveryService : ITelegramDeliveryService
{
    private readonly NewsCollectorDbContext _db;
    private readonly ITelegramApiClient _telegramApi;
    private readonly TelegramBotWorkerOptions _options;

    public TelegramDeliveryService(
        NewsCollectorDbContext db,
        ITelegramApiClient telegramApi,
        IOptions<TelegramBotWorkerOptions> options)
    {
        _db = db;
        _telegramApi = telegramApi;
        _options = options.Value;
    }

    public async Task<TelegramSendResultDto?> QueueNewsAsync(
        Guid newsId,
        Guid channelId,
        CancellationToken cancellationToken = default)
    {
        var news = await _db.NewsItems
            .AsNoTracking()
            .Include(item => item.Source)
            .FirstOrDefaultAsync(item => item.Id == newsId, cancellationToken);

        if (news is null)
        {
            return null;
        }

        var channel = await LoadActiveChannelAsync(channelId, cancellationToken);
        if (channel is null)
        {
            return null;
        }

        var message = TelegramMessageFormatter.FormatNews(news, news.Source.Name);
        return await QueueDeliveryAsync(channel, message, newsId, null, cancellationToken);
    }

    public async Task<TelegramSendResultDto?> QueueRewriteAsync(
        Guid rewriteId,
        Guid channelId,
        CancellationToken cancellationToken = default)
    {
        var rewrite = await _db.NewsRewrites
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == rewriteId, cancellationToken);

        if (rewrite is null)
        {
            return null;
        }

        var channel = await LoadActiveChannelAsync(channelId, cancellationToken);
        if (channel is null)
        {
            return null;
        }

        var message = TelegramMessageFormatter.FormatRewrite(rewrite);
        return await QueueDeliveryAsync(channel, message, null, rewriteId, cancellationToken);
    }

    public async Task<int> ProcessPendingForBotAsync(Guid botId, CancellationToken cancellationToken = default)
    {
        var bot = await _db.TelegramBots.FirstOrDefaultAsync(item => item.Id == botId, cancellationToken);
        if (bot is null || !bot.IsActive)
        {
            return 0;
        }

        var pending = await _db.TelegramDeliveries
            .Include(delivery => delivery.Channel)
            .Where(delivery =>
                delivery.Status == TelegramDeliveryStatus.Pending &&
                delivery.Channel.TelegramBotId == botId &&
                delivery.Channel.IsActive)
            .OrderBy(delivery => delivery.CreatedAt)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            return 0;
        }

        foreach (var delivery in pending)
        {
            try
            {
                var messageId = await _telegramApi.SendHtmlMessageAsync(
                    bot.BotToken,
                    delivery.Channel.ChatId,
                    delivery.MessageText,
                    cancellationToken);

                delivery.Status = TelegramDeliveryStatus.Sent;
                delivery.TelegramMessageId = messageId;
                delivery.SentAt = DateTimeOffset.UtcNow;
                delivery.ErrorMessage = null;
            }
            catch (Exception ex)
            {
                delivery.Status = TelegramDeliveryStatus.Failed;
                delivery.ErrorMessage = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return pending.Count;
    }

    private async Task<TelegramChannel?> LoadActiveChannelAsync(Guid channelId, CancellationToken cancellationToken) =>
        await _db.TelegramChannels
            .Include(channel => channel.Bot)
            .FirstOrDefaultAsync(
                channel => channel.Id == channelId && channel.IsActive && channel.Bot.IsActive,
                cancellationToken);

    private async Task<TelegramSendResultDto> QueueDeliveryAsync(
        TelegramChannel channel,
        string message,
        Guid? newsId,
        Guid? rewriteId,
        CancellationToken cancellationToken)
    {
        var delivery = new TelegramDelivery
        {
            Id = Guid.NewGuid(),
            TelegramChannelId = channel.Id,
            NewsItemId = newsId,
            NewsRewriteId = rewriteId,
            MessageText = message,
            Status = TelegramDeliveryStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.TelegramDeliveries.Add(delivery);
        await _db.SaveChangesAsync(cancellationToken);

        return new TelegramSendResultDto(
            delivery.Id,
            delivery.Status.ToString(),
            "Сообщение поставлено в очередь на отправку");
    }
}

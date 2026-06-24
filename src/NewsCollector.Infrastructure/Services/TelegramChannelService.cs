using Microsoft.EntityFrameworkCore;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Entities;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class TelegramChannelService : ITelegramChannelService
{
    private readonly NewsCollectorDbContext _db;
    private readonly ITelegramApiClient _telegramApi;

    public TelegramChannelService(NewsCollectorDbContext db, ITelegramApiClient telegramApi)
    {
        _db = db;
        _telegramApi = telegramApi;
    }

    public async Task<IReadOnlyList<TelegramChannelDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var channels = await QueryChannels()
            .OrderBy(channel => channel.Name)
            .ToListAsync(cancellationToken);

        return channels.Select(MapChannel).ToList();
    }

    public async Task<IReadOnlyList<TelegramChannelDto>> GetForNewsAsync(
        Guid? sourceId,
        Guid? categoryId,
        CancellationToken cancellationToken = default)
    {
        var channels = await QueryChannels()
            .Where(channel => channel.IsActive && channel.Bot.IsActive)
            .OrderBy(channel => channel.Name)
            .ToListAsync(cancellationToken);

        var filtered = channels
            .Where(channel => MatchesRouting(channel, sourceId, categoryId))
            .Select(MapChannel)
            .ToList();

        return filtered;
    }

    public async Task<TelegramChannelDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var channel = await QueryChannels()
            .FirstOrDefaultAsync(channel => channel.Id == id, cancellationToken);

        return channel is null ? null : MapChannel(channel);
    }

    public async Task<TelegramChannelDto?> CreateAsync(
        CreateTelegramChannelRequest request,
        CancellationToken cancellationToken = default)
    {
        var botExists = await _db.TelegramBots.AnyAsync(bot => bot.Id == request.TelegramBotId, cancellationToken);
        if (!botExists)
        {
            return null;
        }

        var name = request.Name.Trim();
        var chatId = request.ChatId.Trim();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(chatId))
        {
            return null;
        }

        if (await _db.TelegramChannels.AnyAsync(
                channel => channel.TelegramBotId == request.TelegramBotId && channel.ChatId == chatId,
                cancellationToken))
        {
            return null;
        }

        var bot = await _db.TelegramBots.FirstOrDefaultAsync(item => item.Id == request.TelegramBotId, cancellationToken);
        if (bot is null)
        {
            return null;
        }

        try
        {
            await _telegramApi.EnsureChatAccessibleAsync(bot.BotToken, chatId, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException($"Telegram не видит канал {chatId}: {ex.Message}", ex);
        }

        var now = DateTimeOffset.UtcNow;
        var channel = new TelegramChannel
        {
            Id = Guid.NewGuid(),
            TelegramBotId = request.TelegramBotId,
            Name = name,
            ChatId = chatId,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.TelegramChannels.Add(channel);
        await ReplaceRoutingAsync(channel, request.CategoryIds, request.SourceIds, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(channel.Id, cancellationToken);
    }

    public async Task<TelegramChannelDto?> UpdateAsync(
        Guid id,
        UpdateTelegramChannelRequest request,
        CancellationToken cancellationToken = default)
    {
        var channel = await _db.TelegramChannels
            .Include(c => c.ChannelCategories)
            .Include(c => c.ChannelSources)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (channel is null)
        {
            return null;
        }

        var botExists = await _db.TelegramBots.AnyAsync(bot => bot.Id == request.TelegramBotId, cancellationToken);
        if (!botExists)
        {
            return null;
        }

        var name = request.Name.Trim();
        var chatId = request.ChatId.Trim();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(chatId))
        {
            return null;
        }

        if (await _db.TelegramChannels.AnyAsync(
                c => c.TelegramBotId == request.TelegramBotId && c.ChatId == chatId && c.Id != id,
                cancellationToken))
        {
            return null;
        }

        var bot = await _db.TelegramBots.FirstOrDefaultAsync(item => item.Id == request.TelegramBotId, cancellationToken);
        if (bot is null)
        {
            return null;
        }

        try
        {
            await _telegramApi.EnsureChatAccessibleAsync(bot.BotToken, chatId, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException($"Telegram не видит канал {chatId}: {ex.Message}", ex);
        }

        channel.TelegramBotId = request.TelegramBotId;
        channel.Name = name;
        channel.ChatId = chatId;
        channel.IsActive = request.IsActive;
        channel.UpdatedAt = DateTimeOffset.UtcNow;

        await ReplaceRoutingAsync(channel, request.CategoryIds, request.SourceIds, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(channel.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var channel = await _db.TelegramChannels.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (channel is null)
        {
            return false;
        }

        _db.TelegramChannels.Remove(channel);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private IQueryable<TelegramChannel> QueryChannels() =>
        _db.TelegramChannels
            .AsNoTracking()
            .Include(channel => channel.Bot)
            .Include(channel => channel.ChannelCategories)
            .ThenInclude(item => item.Category)
            .Include(channel => channel.ChannelSources)
            .ThenInclude(item => item.Source);

    private async Task ReplaceRoutingAsync(
        TelegramChannel channel,
        IReadOnlyList<Guid> categoryIds,
        IReadOnlyList<Guid> sourceIds,
        CancellationToken cancellationToken)
    {
        _db.TelegramChannelCategories.RemoveRange(channel.ChannelCategories);
        _db.TelegramChannelSources.RemoveRange(channel.ChannelSources);

        var uniqueCategoryIds = categoryIds.Distinct().ToList();
        var uniqueSourceIds = sourceIds.Distinct().ToList();

        if (uniqueCategoryIds.Count > 0)
        {
            var existingCategoryIds = await _db.Categories
                .Where(category => uniqueCategoryIds.Contains(category.Id))
                .Select(category => category.Id)
                .ToListAsync(cancellationToken);

            foreach (var categoryId in existingCategoryIds)
            {
                channel.ChannelCategories.Add(new TelegramChannelCategory
                {
                    TelegramChannelId = channel.Id,
                    CategoryId = categoryId
                });
            }
        }

        if (uniqueSourceIds.Count > 0)
        {
            var existingSourceIds = await _db.Sources
                .Where(source => uniqueSourceIds.Contains(source.Id))
                .Select(source => source.Id)
                .ToListAsync(cancellationToken);

            foreach (var sourceId in existingSourceIds)
            {
                channel.ChannelSources.Add(new TelegramChannelSource
                {
                    TelegramChannelId = channel.Id,
                    SourceId = sourceId
                });
            }
        }
    }

    private static bool MatchesRouting(TelegramChannel channel, Guid? sourceId, Guid? categoryId)
    {
        var hasCategoryRules = channel.ChannelCategories.Count > 0;
        var hasSourceRules = channel.ChannelSources.Count > 0;

        if (!hasCategoryRules && !hasSourceRules)
        {
            return true;
        }

        var categoryMatch = !hasCategoryRules
            || (categoryId.HasValue && channel.ChannelCategories.Any(item => item.CategoryId == categoryId.Value));

        var sourceMatch = !hasSourceRules
            || (sourceId.HasValue && channel.ChannelSources.Any(item => item.SourceId == sourceId.Value));

        return categoryMatch && sourceMatch;
    }

    private static TelegramChannelDto MapChannel(TelegramChannel channel) =>
        new(
            channel.Id,
            channel.TelegramBotId,
            channel.Bot.Name,
            channel.Name,
            channel.ChatId,
            channel.IsActive,
            channel.ChannelCategories.Select(item => item.CategoryId).ToList(),
            channel.ChannelSources.Select(item => item.SourceId).ToList(),
            channel.ChannelCategories.Select(item => item.Category.Name).ToList(),
            channel.ChannelSources.Select(item => item.Source.Name).ToList(),
            channel.CreatedAt,
            channel.UpdatedAt);
}

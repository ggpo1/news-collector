using Microsoft.EntityFrameworkCore;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Entities;
using NewsCollector.Domain.Enums;
using NewsCollector.Infrastructure.Docker;
using NewsCollector.Infrastructure.Persistence;
using NewsCollector.Infrastructure.Telegram;

namespace NewsCollector.Infrastructure.Services;

public sealed class TelegramBotService : ITelegramBotService
{
    private readonly NewsCollectorDbContext _db;
    private readonly ITelegramBotOrchestrator _orchestrator;

    public TelegramBotService(NewsCollectorDbContext db, ITelegramBotOrchestrator orchestrator)
    {
        _db = db;
        _orchestrator = orchestrator;
    }

    public async Task<IReadOnlyList<TelegramBotDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var bots = await _db.TelegramBots
            .AsNoTracking()
            .OrderBy(bot => bot.Name)
            .Select(bot => new
            {
                Bot = bot,
                ChannelCount = bot.Channels.Count
            })
            .ToListAsync(cancellationToken);

        return bots.Select(item => MapBot(item.Bot, item.ChannelCount)).ToList();
    }

    public async Task<TelegramBotDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bot = await _db.TelegramBots
            .AsNoTracking()
            .Include(b => b.Channels)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        return bot is null ? null : MapBot(bot, bot.Channels.Count);
    }

    public async Task<TelegramBotDto?> CreateAsync(
        CreateTelegramBotRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        var token = request.BotToken.Trim();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var bot = new TelegramBot
        {
            Id = Guid.NewGuid(),
            Name = name,
            BotToken = token,
            IsActive = request.IsActive,
            ContainerName = DockerCliTelegramBotOrchestrator.BuildContainerName(Guid.Empty),
            ContainerStatus = TelegramBotContainerStatus.Stopped,
            CreatedAt = now,
            UpdatedAt = now
        };

        bot.ContainerName = DockerCliTelegramBotOrchestrator.BuildContainerName(bot.Id);

        _db.TelegramBots.Add(bot);
        await _db.SaveChangesAsync(cancellationToken);

        await StartContainerAsync(bot, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return MapBot(bot, 0);
    }

    public async Task<TelegramBotDto?> UpdateAsync(
        Guid id,
        UpdateTelegramBotRequest request,
        CancellationToken cancellationToken = default)
    {
        var bot = await _db.TelegramBots
            .Include(b => b.Channels)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (bot is null)
        {
            return null;
        }

        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        bot.Name = name;
        bot.IsActive = request.IsActive;
        if (!string.IsNullOrWhiteSpace(request.BotToken))
        {
            bot.BotToken = request.BotToken.Trim();
        }

        bot.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        if (bot.IsActive && bot.ContainerStatus != TelegramBotContainerStatus.Running)
        {
            await StartContainerAsync(bot, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }
        else if (!bot.IsActive && bot.ContainerStatus == TelegramBotContainerStatus.Running)
        {
            await _orchestrator.StopBotContainerAsync(bot.Id, cancellationToken);
            bot.ContainerStatus = TelegramBotContainerStatus.Stopped;
            bot.ContainerError = null;
            bot.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return MapBot(bot, bot.Channels.Count);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bot = await _db.TelegramBots.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (bot is null)
        {
            return false;
        }

        await _orchestrator.StopBotContainerAsync(bot.Id, cancellationToken);
        _db.TelegramBots.Remove(bot);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<TelegramBotDto?> RestartContainerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bot = await _db.TelegramBots
            .Include(b => b.Channels)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (bot is null)
        {
            return null;
        }

        await StartContainerAsync(bot, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return MapBot(bot, bot.Channels.Count);
    }

    private async Task StartContainerAsync(TelegramBot bot, CancellationToken cancellationToken)
    {
        if (!bot.IsActive)
        {
            return;
        }

        var result = await _orchestrator.StartBotContainerAsync(bot.Id, cancellationToken);
        bot.UpdatedAt = DateTimeOffset.UtcNow;

        if (result.Skipped)
        {
            bot.ContainerStatus = TelegramBotContainerStatus.Stopped;
            bot.ContainerError = result.ErrorMessage;
            return;
        }

        if (result.Success)
        {
            bot.ContainerStatus = TelegramBotContainerStatus.Running;
            bot.ContainerError = null;
            return;
        }

        bot.ContainerStatus = TelegramBotContainerStatus.Error;
        bot.ContainerError = result.ErrorMessage;
    }

    private static TelegramBotDto MapBot(TelegramBot bot, int channelCount) =>
        new(
            bot.Id,
            bot.Name,
            TelegramMessageFormatter.MaskToken(bot.BotToken),
            bot.IsActive,
            bot.ContainerStatus.ToString(),
            bot.ContainerName,
            bot.ContainerError,
            channelCount,
            bot.CreatedAt,
            bot.UpdatedAt);
}

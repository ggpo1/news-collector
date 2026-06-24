using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Options;

namespace NewsCollector.TelegramBot;

public sealed class TelegramBotWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelegramBotWorker> _logger;
    private readonly TelegramBotWorkerOptions _options;

    public TelegramBotWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<TelegramBotWorkerOptions> options,
        ILogger<TelegramBotWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.BotId.HasValue)
        {
            _logger.LogError("TelegramBot:BotId is not configured. Worker cannot start.");
            return;
        }

        var botId = _options.BotId.Value;
        _logger.LogInformation(
            "Telegram bot worker started for {BotId} (polling every {IntervalSeconds}s)",
            botId,
            _options.PollingIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessAsync(botId, stoppingToken);

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ProcessAsync(Guid botId, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var deliveryService = scope.ServiceProvider.GetRequiredService<ITelegramDeliveryService>();
            var processed = await deliveryService.ProcessPendingForBotAsync(botId, cancellationToken);

            if (processed > 0)
            {
                _logger.LogInformation("Telegram delivery cycle completed: {Count} messages", processed);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Telegram delivery cycle failed for bot {BotId}", botId);
        }
    }
}

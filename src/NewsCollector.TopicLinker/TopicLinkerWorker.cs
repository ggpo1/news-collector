using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Options;

namespace NewsCollector.TopicLinker;

public sealed class TopicLinkerWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TopicLinkerWorker> _logger;
    private readonly TopicLinkerOptions _options;

    public TopicLinkerWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<TopicLinkerOptions> options,
        ILogger<TopicLinkerWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Topic linker started (polling every {IntervalSeconds}s, lookback {LookbackHours}h)",
            _options.PollingIntervalSeconds,
            _options.LookbackHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            await LinkTopicsAsync(stoppingToken);

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

    private async Task LinkTopicsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var linkingService = scope.ServiceProvider.GetRequiredService<ITopicLinkingService>();
            var created = await linkingService.LinkSameTopicNewsAsync(cancellationToken);

            var storySync = scope.ServiceProvider.GetRequiredService<IStorySyncService>();
            var synced = await storySync.SyncFromLinksAsync(cancellationToken);

            if (created > 0 || synced > 0)
            {
                _logger.LogInformation(
                    "Topic linking cycle completed: {CreatedCount} new links, {SyncedCount} stories synced",
                    created,
                    synced);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Topic linking cycle failed");
        }
    }
}

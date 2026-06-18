using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Options;

namespace NewsCollector.Worker;

public sealed class CollectorWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CollectorWorker> _logger;
    private readonly CollectorOptions _options;

    public CollectorWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<CollectorOptions> options,
        ILogger<CollectorWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "News Collector worker started (polling every {IntervalSeconds}s)",
            _options.PollingIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            await CollectAsync(stoppingToken);

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

    private async Task CollectAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();

            var ingestionService = scope.ServiceProvider.GetRequiredService<INewsIngestionService>();
            var enrichmentService = scope.ServiceProvider.GetRequiredService<IArticleContentEnrichmentService>();

            var newItemsCount = await ingestionService.CollectPendingSourcesAsync(cancellationToken);
            var enrichedCount = await enrichmentService.EnrichPendingArticlesAsync(cancellationToken);

            if (newItemsCount > 0 || enrichedCount > 0)
            {
                _logger.LogInformation(
                    "Cycle completed: {NewItemsCount} new RSS items, {EnrichedCount} articles with full text",
                    newItemsCount,
                    enrichedCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Collection cycle failed");
        }
    }
}

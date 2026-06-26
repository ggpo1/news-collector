using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Options;

namespace NewsCollector.NewsSearchIndexer;

public sealed class NewsSearchIndexerWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NewsSearchIndexerWorker> _logger;
    private readonly NewsSearchIndexerOptions _options;

    public NewsSearchIndexerWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<NewsSearchIndexerOptions> options,
        ILogger<NewsSearchIndexerWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "News search indexer started (polling every {IntervalSeconds}s, batch size {BatchSize})",
            _options.PollingIntervalSeconds,
            _options.BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            await IndexAsync(stoppingToken);

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

    private async Task IndexAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var indexService = scope.ServiceProvider.GetRequiredService<INewsSearchIndexService>();
            var indexed = await indexService.IndexPendingNewsAsync(cancellationToken);

            if (indexed > 0)
            {
                _logger.LogInformation("Search index cycle completed: {Count} items", indexed);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "News search indexing cycle failed");
        }
    }
}

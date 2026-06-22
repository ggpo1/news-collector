using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Options;

namespace NewsCollector.NewsEntityExtractor;

public sealed class NewsEntityExtractorWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NewsEntityExtractorWorker> _logger;
    private readonly NewsEntityExtractorOptions _options;

    public NewsEntityExtractorWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<NewsEntityExtractorOptions> options,
        ILogger<NewsEntityExtractorWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "News entity extractor started (polling every {IntervalSeconds}s, batch size {BatchSize})",
            _options.PollingIntervalSeconds,
            _options.BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ExtractAsync(stoppingToken);

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

    private async Task ExtractAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var extractionService = scope.ServiceProvider.GetRequiredService<INewsEntityExtractionService>();
            var processed = await extractionService.ExtractPendingNewsAsync(cancellationToken);

            if (processed > 0)
            {
                _logger.LogInformation("Entity extraction cycle completed: {Count} items", processed);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "News entity extraction cycle failed");
        }
    }
}

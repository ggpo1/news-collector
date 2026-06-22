using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Options;

namespace NewsCollector.NewsToneAnalyzer;

public sealed class NewsToneAnalyzerWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NewsToneAnalyzerWorker> _logger;
    private readonly NewsToneAnalyzerOptions _options;

    public NewsToneAnalyzerWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<NewsToneAnalyzerOptions> options,
        ILogger<NewsToneAnalyzerWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "News tone analyzer started (polling every {IntervalSeconds}s, batch size {BatchSize})",
            _options.PollingIntervalSeconds,
            _options.BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            await AnalyzeAsync(stoppingToken);

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

    private async Task AnalyzeAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var toneService = scope.ServiceProvider.GetRequiredService<INewsToneAnalysisService>();
            var analyzed = await toneService.AnalyzePendingNewsAsync(cancellationToken);

            if (analyzed > 0)
            {
                _logger.LogInformation("Tone analysis cycle completed: {Count} items", analyzed);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "News tone analysis cycle failed");
        }
    }
}

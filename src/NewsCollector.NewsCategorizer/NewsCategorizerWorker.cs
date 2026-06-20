using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Options;

namespace NewsCollector.NewsCategorizer;

public sealed class NewsCategorizerWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NewsCategorizerWorker> _logger;
    private readonly NewsCategorizerOptions _options;

    public NewsCategorizerWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<NewsCategorizerOptions> options,
        ILogger<NewsCategorizerWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "News categorizer started (polling every {IntervalSeconds}s, batch size {BatchSize})",
            _options.PollingIntervalSeconds,
            _options.BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            await CategorizeAsync(stoppingToken);

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

    private async Task CategorizeAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var categorizationService = scope.ServiceProvider.GetRequiredService<INewsCategorizationService>();
            var categorized = await categorizationService.CategorizePendingNewsAsync(cancellationToken);

            if (categorized > 0)
            {
                _logger.LogInformation("Categorization cycle completed: {Count} items", categorized);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "News categorization cycle failed");
        }
    }
}

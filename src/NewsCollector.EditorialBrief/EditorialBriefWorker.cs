using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Options;

namespace NewsCollector.EditorialBrief;

public sealed class EditorialBriefWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EditorialBriefWorker> _logger;
    private readonly EditorialBriefOptions _options;

    public EditorialBriefWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<EditorialBriefOptions> options,
        ILogger<EditorialBriefWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Editorial brief worker started (check every {IntervalSeconds}s, schedule UTC hours: {Hours})",
            _options.PollingIntervalSeconds,
            string.Join(", ", _options.ScheduleHoursUtc));

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunCycleAsync(stoppingToken);

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

    private async Task RunCycleAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var briefService = scope.ServiceProvider.GetRequiredService<IEditorialBriefService>();
            var generated = await briefService.TryGenerateScheduledAsync(cancellationToken);

            if (generated > 0)
            {
                _logger.LogInformation("Editorial brief cycle completed: {Count} brief(s) generated", generated);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Editorial brief cycle failed");
        }
    }
}

namespace NewsCollector.Application.Abstractions;

public interface ITelegramBotOrchestrator
{
    Task<OrchestratorResult> StartBotContainerAsync(Guid botId, CancellationToken cancellationToken = default);

    Task StopBotContainerAsync(Guid botId, CancellationToken cancellationToken = default);
}

public sealed record OrchestratorResult(bool Success, bool Skipped, string? ErrorMessage);

namespace NewsCollector.Application.Abstractions;

public interface INewsIngestionService
{
    Task<int> CollectPendingSourcesAsync(CancellationToken cancellationToken = default);
}

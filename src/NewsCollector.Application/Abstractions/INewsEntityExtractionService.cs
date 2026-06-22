namespace NewsCollector.Application.Abstractions;

public interface INewsEntityExtractionService
{
    Task<int> ExtractPendingNewsAsync(CancellationToken cancellationToken = default);
}

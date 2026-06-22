namespace NewsCollector.Application.Abstractions;

public interface INewsToneAnalysisService
{
    /// <summary>
    /// Computes tone coefficient for news items where it is not set yet.
    /// </summary>
    /// <returns>Number of news items analyzed in this batch.</returns>
    Task<int> AnalyzePendingNewsAsync(CancellationToken cancellationToken = default);
}

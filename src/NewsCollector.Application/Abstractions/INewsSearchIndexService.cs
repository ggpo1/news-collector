namespace NewsCollector.Application.Abstractions;

public interface INewsSearchIndexService
{
    /// <summary>
    /// Indexes news items where <see cref="Domain.Entities.NewsItem.SearchIndexedAt"/> is null (newest first).
    /// </summary>
    /// <returns>Number of news items indexed in this batch.</returns>
    Task<int> IndexPendingNewsAsync(CancellationToken cancellationToken = default);
}

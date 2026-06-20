namespace NewsCollector.Application.Abstractions;

public interface INewsCategorizationService
{
    /// <summary>
    /// Classifies uncategorized news items and sets <see cref="Domain.Entities.NewsItem.CategoryId"/>.
    /// </summary>
    /// <returns>Number of news items categorized in this batch.</returns>
    Task<int> CategorizePendingNewsAsync(CancellationToken cancellationToken = default);
}

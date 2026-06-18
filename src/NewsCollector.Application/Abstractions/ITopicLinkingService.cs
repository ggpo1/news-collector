namespace NewsCollector.Application.Abstractions;

public interface ITopicLinkingService
{
    /// <summary>
    /// Finds news on the same topic and persists links in news_links.
    /// </summary>
    /// <returns>Number of new links created.</returns>
    Task<int> LinkSameTopicNewsAsync(CancellationToken cancellationToken = default);
}

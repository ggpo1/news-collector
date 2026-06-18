using NewsCollector.Application.Models;

namespace NewsCollector.Application.Abstractions;

public interface IRssFeedReader
{
    Task<IReadOnlyList<ParsedFeedItem>> ReadAsync(string feedUrl, CancellationToken cancellationToken = default);
}

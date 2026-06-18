using NewsCollector.Application.Models;

namespace NewsCollector.Infrastructure.Feeds;

internal static class FeedItemDeduplicator
{
    public static IReadOnlyList<ParsedFeedItem> Deduplicate(IReadOnlyList<ParsedFeedItem> items)
    {
        if (items.Count <= 1)
        {
            return items;
        }

        var byExternalId = new Dictionary<string, ParsedFeedItem>(StringComparer.Ordinal);

        foreach (var item in items)
        {
            var key = NormalizeKey(item.ExternalId);
            if (!byExternalId.TryGetValue(key, out var existing) || IsNewer(item, existing))
            {
                byExternalId[key] = item;
            }
        }

        var byUrl = new Dictionary<string, ParsedFeedItem>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in byExternalId.Values)
        {
            var key = NormalizeKey(item.Url);
            if (!byUrl.TryGetValue(key, out var existing) || IsNewer(item, existing))
            {
                byUrl[key] = item;
            }
        }

        return byUrl.Values.ToList();
    }

    private static bool IsNewer(ParsedFeedItem candidate, ParsedFeedItem current)
    {
        if (candidate.PublishedAt is null)
        {
            return false;
        }

        if (current.PublishedAt is null)
        {
            return true;
        }

        return candidate.PublishedAt > current.PublishedAt;
    }

    private static string NormalizeKey(string value) => value.Trim();
}

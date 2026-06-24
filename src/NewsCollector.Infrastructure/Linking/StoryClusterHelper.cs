using NewsCollector.Domain.Entities;

namespace NewsCollector.Infrastructure.Linking;

public static class StoryClusterHelper
{
    public static string BuildClusterKey(IEnumerable<Guid> newsIds) =>
        newsIds.Min().ToString("N");

    public static NewsItem SelectPrimary(IReadOnlyList<NewsItem> articles) =>
        articles
            .OrderByDescending(ScorePrimaryCandidate)
            .ThenBy(item => item.PublishedAt ?? item.FetchedAt)
            .First();

    public static int ScorePrimaryCandidate(NewsItem item)
    {
        var score = 0;
        if (!string.IsNullOrWhiteSpace(item.Content))
        {
            score += 1_000;
        }

        if (item.ContentFetchedAt.HasValue)
        {
            score += 100;
        }

        if (!string.IsNullOrWhiteSpace(item.Summary))
        {
            score += 10;
        }

        return score;
    }
}

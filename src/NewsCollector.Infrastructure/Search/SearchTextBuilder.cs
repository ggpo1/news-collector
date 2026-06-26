using NewsCollector.Domain.Entities;

namespace NewsCollector.Infrastructure.Search;

internal static class SearchTextBuilder
{
    public static string BuildNewsBody(NewsItem news, int maxBodyCharacters)
    {
        if (!string.IsNullOrWhiteSpace(news.Content))
        {
            var content = news.Content.Trim();
            return content.Length <= maxBodyCharacters
                ? content
                : content[..maxBodyCharacters];
        }

        return news.Summary?.Trim() ?? string.Empty;
    }
}

using System.Text.RegularExpressions;

namespace NewsCollector.Infrastructure.Linking;

public static partial class TitleSimilarityCalculator
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "и", "в", "во", "на", "с", "со", "к", "ко", "от", "до", "по", "за", "из", "у", "о", "об", "для",
        "при", "не", "но", "а", "или", "что", "как", "это", "так", "же", "уже", "ещё", "еще", "бы", "ли",
        "все", "всё", "его", "ее", "её", "их", "он", "она", "они", "мы", "вы", "ты", "я", "был", "была",
        "были", "будет", "будут", "после", "перед", "между", "через", "под", "над", "без", "про", "обо",
        "the", "a", "an", "and", "or", "in", "on", "at", "to", "for", "of", "with", "by", "from", "is",
        "are", "was", "were", "be", "been", "has", "have", "had", "not", "but", "as", "it", "its",
        "новости", "news", "сообщили", "заявил", "заявила", "рассказал", "стало", "известно"
    };

    public static decimal ComputeSimilarity(string left, string? rightSummary, string right, string? leftSummary)
    {
        var leftTokens = Tokenize(CombineText(left, leftSummary));
        var rightTokens = Tokenize(CombineText(right, rightSummary));

        if (leftTokens.Count == 0 || rightTokens.Count == 0)
        {
            return 0m;
        }

        var intersection = leftTokens.Intersect(rightTokens).Count();
        var union = leftTokens.Union(rightTokens).Count();

        return union == 0 ? 0m : Math.Round((decimal)intersection / union, 4);
    }

    public static int CountSharedTokens(string left, string? leftSummary, string right, string? rightSummary)
    {
        var leftTokens = Tokenize(CombineText(left, leftSummary));
        var rightTokens = Tokenize(CombineText(right, rightSummary));
        return leftTokens.Intersect(rightTokens).Count();
    }

    public static string CombineTextForEmbedding(string title, string? summary) =>
        CombineText(title, summary);

    private static string CombineText(string title, string? summary)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            return title;
        }

        return $"{title} {summary}";
    }

    private static HashSet<string> Tokenize(string text)
    {
        var normalized = NonWordRegex().Replace(text.ToLowerInvariant(), " ");
        var tokens = normalized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => token.Length >= 3)
            .Where(token => !StopWords.Contains(token))
            .Where(token => !token.All(char.IsDigit));

        return tokens.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    [GeneratedRegex(@"[^\p{L}\p{Nd}]+", RegexOptions.Compiled)]
    private static partial Regex NonWordRegex();
}

using NewsCollector.Application.Options;

namespace NewsCollector.Infrastructure.Linking;

internal enum SourceRegion
{
    Unknown,
    Ru,
    Western
}

internal static class SourceRegionClassifier
{
    public static SourceRegion Classify(string sourceName, string sourceUrl, EditorialBriefOptions options)
    {
        var haystack = $"{sourceName} {sourceUrl}".ToLowerInvariant();

        if (options.RuSourcePatterns.Any(pattern => haystack.Contains(pattern, StringComparison.Ordinal)))
        {
            return SourceRegion.Ru;
        }

        if (options.WesternSourcePatterns.Any(pattern => haystack.Contains(pattern, StringComparison.Ordinal)))
        {
            return SourceRegion.Western;
        }

        return SourceRegion.Unknown;
    }

    public static bool HasOnlyRegion(
        IEnumerable<(string SourceName, string SourceUrl)> sources,
        SourceRegion region,
        EditorialBriefOptions options)
    {
        var classified = sources
            .Select(source => Classify(source.SourceName, source.SourceUrl, options))
            .Where(item => item != SourceRegion.Unknown)
            .ToList();

        return classified.Count >= 2 && classified.All(item => item == region);
    }
}

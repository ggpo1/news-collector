using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Options;
using NewsCollector.Domain.Entities;
using NewsCollector.Domain.Enums;
using NewsCollector.Infrastructure.Linking;

namespace NewsCollector.Infrastructure.Search;

public sealed class ContentLanguageResolver : IContentLanguageResolver
{
    private readonly EditorialBriefOptions _options;

    public ContentLanguageResolver(IOptions<EditorialBriefOptions> options)
    {
        _options = options.Value;
    }

    public ContentLanguage Resolve(Source source, string title, string? summary = null, string? content = null)
    {
        if (source.DefaultLanguage.HasValue)
        {
            return source.DefaultLanguage.Value;
        }

        var region = SourceRegionClassifier.Classify(source.Name, source.Url, _options);
        if (region == SourceRegion.Ru)
        {
            return ContentLanguage.Ru;
        }

        if (region == SourceRegion.Western)
        {
            return ContentLanguage.En;
        }

        return HasCyrillicDominance($"{title} {summary} {content}")
            ? ContentLanguage.Ru
            : ContentLanguage.En;
    }

    private static bool HasCyrillicDominance(string text)
    {
        var letters = 0;
        var cyrillic = 0;

        foreach (var character in text)
        {
            if (!char.IsLetter(character))
            {
                continue;
            }

            letters++;
            if (character is >= '\u0400' and <= '\u04FF')
            {
                cyrillic++;
            }
        }

        return letters > 0 && (double)cyrillic / letters >= 0.25;
    }
}

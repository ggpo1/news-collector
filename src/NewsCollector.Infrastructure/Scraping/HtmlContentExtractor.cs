using System.Text;
using AngleSharp;
using AngleSharp.Dom;
using NewsCollector.Application.Abstractions;

namespace NewsCollector.Infrastructure.Scraping;

public sealed class HtmlContentExtractor : IHtmlContentExtractor
{
    private static readonly IBrowsingContext Context = BrowsingContext.New(Configuration.Default);

    public async Task<string?> ExtractAsync(
        string html,
        string cssSelector,
        CancellationToken cancellationToken = default)
    {
        var document = await Context.OpenAsync(req => req.Content(html), cancellationToken);
        var elements = document.QuerySelectorAll(cssSelector);

        if (elements.Length == 0)
        {
            return null;
        }

        var parts = new List<string>();

        foreach (var element in elements)
        {
            var text = NormalizeText(element);
            if (!string.IsNullOrWhiteSpace(text))
            {
                parts.Add(text);
            }
        }

        return parts.Count == 0 ? null : string.Join("\n\n", parts);
    }

    private static string NormalizeText(IElement element)
    {
        var builder = new StringBuilder();

        foreach (var node in element.ChildNodes)
        {
            if (node is IText textNode)
            {
                builder.Append(textNode.Data);
            }
            else if (node is IElement childElement)
            {
                builder.Append(NormalizeText(childElement));
                if (childElement.TagName.Equals("P", StringComparison.OrdinalIgnoreCase))
                {
                    builder.Append('\n');
                }
            }
        }

        return builder.ToString().Trim();
    }
}

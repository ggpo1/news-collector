using System.Text.Json;
using System.Text.RegularExpressions;

namespace NewsCollector.Infrastructure.Ai;

internal static partial class OllamaCategoryResponseParser
{
    public static string ParseSlug(string rawResponse)
    {
        var json = ExtractJson(rawResponse);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (!root.TryGetProperty("categorySlug", out var property))
        {
            throw new InvalidOperationException("Ollama response is missing 'categorySlug'.");
        }

        var value = property.GetString()?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("Ollama response field 'categorySlug' is empty.");
        }

        return value;
    }

    private static string ExtractJson(string rawResponse)
    {
        var trimmed = rawResponse.Trim();

        var fenced = CodeFenceRegex().Match(trimmed);
        if (fenced.Success)
        {
            return fenced.Groups["json"].Value.Trim();
        }

        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            return trimmed[start..(end + 1)];
        }

        return trimmed;
    }

    [GeneratedRegex(@"```(?:json)?\s*(?<json>.*?)\s*```", RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex CodeFenceRegex();
}

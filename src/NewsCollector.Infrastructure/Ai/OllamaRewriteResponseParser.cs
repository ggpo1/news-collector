using System.Text.Json;
using System.Text.RegularExpressions;
using NewsCollector.Application.Dtos;

namespace NewsCollector.Infrastructure.Ai;

internal static partial class OllamaRewriteResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static AiNewsRewriteResultDto Parse(string rawResponse)
    {
        var json = ExtractJson(rawResponse);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var title = GetRequiredString(root, "title");
        var summary = GetOptionalString(root, "summary");
        var content = GetRequiredString(root, "content");

        return new AiNewsRewriteResultDto(title, summary, content);
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

    private static string GetRequiredString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property))
        {
            throw new InvalidOperationException($"Ollama response is missing '{propertyName}'.");
        }

        var value = property.GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Ollama response field '{propertyName}' is empty.");
        }

        return value;
    }

    private static string? GetOptionalString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        var value = property.GetString()?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    [GeneratedRegex(@"```(?:json)?\s*(?<json>.*?)\s*```", RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex CodeFenceRegex();
}

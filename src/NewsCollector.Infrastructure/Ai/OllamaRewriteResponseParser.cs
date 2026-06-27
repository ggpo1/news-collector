using System.Text.Json;
using NewsCollector.Application.Dtos;

namespace NewsCollector.Infrastructure.Ai;

internal static class OllamaRewriteResponseParser
{
    public static AiNewsRewriteResultDto Parse(string rawResponse)
    {
        var json = OllamaJsonExtractor.ExtractFirstJsonObject(rawResponse);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var title = GetRequiredString(root, "title");
        var summary = GetOptionalString(root, "summary");
        var content = GetRequiredString(root, "content");

        return new AiNewsRewriteResultDto(title, summary, content);
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
}

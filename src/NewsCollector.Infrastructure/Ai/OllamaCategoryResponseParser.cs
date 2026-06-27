using System.Text.Json;

namespace NewsCollector.Infrastructure.Ai;

internal static class OllamaCategoryResponseParser
{
    public static string ParseSlug(string rawResponse)
    {
        var json = OllamaJsonExtractor.ExtractFirstJsonObject(rawResponse);
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
}

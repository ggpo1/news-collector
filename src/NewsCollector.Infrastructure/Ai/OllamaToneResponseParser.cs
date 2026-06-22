using System.Text.Json;

namespace NewsCollector.Infrastructure.Ai;

internal static class OllamaToneResponseParser
{
    public static decimal ParseCoefficient(string rawResponse)
    {
        var json = ExtractJson(rawResponse);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (!root.TryGetProperty("toneCoefficient", out var property))
        {
            throw new InvalidOperationException("Ollama response is missing 'toneCoefficient'.");
        }

        decimal value;
        if (property.ValueKind == JsonValueKind.Number)
        {
            value = property.GetDecimal();
        }
        else
        {
            var text = property.GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(text) || !decimal.TryParse(text, out value))
            {
                throw new InvalidOperationException("Ollama response field 'toneCoefficient' is invalid.");
            }
        }

        return Math.Clamp(value, -1m, 1m);
    }

    private static string ExtractJson(string rawResponse)
    {
        var trimmed = rawResponse.Trim();
        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            return trimmed[start..(end + 1)];
        }

        return trimmed;
    }
}

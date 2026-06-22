using System.Text.Json;
using System.Text.RegularExpressions;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Infrastructure.Ai;

internal sealed record ExtractedEntity(string Name, NamedEntityType Type);

internal static partial class OllamaEntityResponseParser
{
    private static readonly Dictionary<string, NamedEntityType> TypeAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["person"] = NamedEntityType.Person,
        ["people"] = NamedEntityType.Person,
        ["human"] = NamedEntityType.Person,
        ["company"] = NamedEntityType.Company,
        ["corporation"] = NamedEntityType.Company,
        ["business"] = NamedEntityType.Company,
        ["country"] = NamedEntityType.Country,
        ["state"] = NamedEntityType.Country,
        ["nation"] = NamedEntityType.Country,
        ["organization"] = NamedEntityType.Organization,
        ["organisation"] = NamedEntityType.Organization,
        ["org"] = NamedEntityType.Organization,
        ["ngo"] = NamedEntityType.Organization,
        ["location"] = NamedEntityType.Location,
        ["place"] = NamedEntityType.Location,
        ["city"] = NamedEntityType.Location,
        ["region"] = NamedEntityType.Location,
        ["event"] = NamedEntityType.Event,
        ["other"] = NamedEntityType.Other
    };

    public static IReadOnlyList<ExtractedEntity> ParseEntities(string rawResponse, int maxEntities)
    {
        var json = ExtractJson(rawResponse);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (!root.TryGetProperty("entities", out var entitiesProperty)
            || entitiesProperty.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Ollama response is missing 'entities' array.");
        }

        var result = new List<ExtractedEntity>();
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var element in entitiesProperty.EnumerateArray())
        {
            if (!TryParseEntity(element, out var entity))
            {
                continue;
            }

            var key = EntityNormalization.CreateNormalizedKey(entity.Name, entity.Type);
            if (!seenKeys.Add(key))
            {
                continue;
            }

            result.Add(entity);

            if (result.Count >= maxEntities)
            {
                break;
            }
        }

        return result;
    }

    private static bool TryParseEntity(JsonElement element, out ExtractedEntity entity)
    {
        entity = default!;

        if (!element.TryGetProperty("name", out var nameProperty))
        {
            return false;
        }

        var name = nameProperty.GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 256)
        {
            return false;
        }

        var type = NamedEntityType.Other;
        if (element.TryGetProperty("type", out var typeProperty))
        {
            var typeText = typeProperty.ValueKind == JsonValueKind.String
                ? typeProperty.GetString()
                : typeProperty.ToString();

            if (!string.IsNullOrWhiteSpace(typeText)
                && TypeAliases.TryGetValue(typeText.Trim(), out var parsedType))
            {
                type = parsedType;
            }
        }

        entity = new ExtractedEntity(name, type);
        return true;
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

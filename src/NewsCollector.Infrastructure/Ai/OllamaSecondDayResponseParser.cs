using System.Text.Json;
using System.Text.RegularExpressions;
using NewsCollector.Application.Dtos;

namespace NewsCollector.Infrastructure.Ai;

internal static partial class OllamaSecondDayResponseParser
{
    public static ParsedSecondDayResponse Parse(string rawResponse)
    {
        var json = ExtractJson(rawResponse);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        return new ParsedSecondDayResponse(
            ParseHistoricalParallels(root),
            ParseStakeholders(root),
            ParseNumberContradictions(root),
            ParseSuggestedAngles(root));
    }

    private static IReadOnlyList<ParsedHistoricalParallel> ParseHistoricalParallels(JsonElement root)
    {
        if (!root.TryGetProperty("historicalParallels", out var array)
            || array.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var result = new List<ParsedHistoricalParallel>();
        foreach (var item in array.EnumerateArray())
        {
            var newsIdText = GetString(item, "newsId");
            if (!Guid.TryParse(newsIdText, out var newsId))
            {
                continue;
            }

            var parallelSummary = GetString(item, "parallelSummary");
            var structuralSimilarity = GetString(item, "structuralSimilarity");
            if (string.IsNullOrWhiteSpace(parallelSummary) && string.IsNullOrWhiteSpace(structuralSimilarity))
            {
                continue;
            }

            result.Add(new ParsedHistoricalParallel(
                newsId,
                parallelSummary ?? string.Empty,
                structuralSimilarity ?? string.Empty));
        }

        return result;
    }

    private static IReadOnlyList<StakeholderAngleDto> ParseStakeholders(JsonElement root)
    {
        if (!root.TryGetProperty("stakeholders", out var array)
            || array.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var result = new List<StakeholderAngleDto>();
        foreach (var item in array.EnumerateArray())
        {
            var name = GetString(item, "name");
            var reason = GetString(item, "reason");
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(reason))
            {
                continue;
            }

            result.Add(new StakeholderAngleDto(
                name.Trim(),
                GetString(item, "type") ?? "other",
                GetString(item, "role") ?? "neutral",
                reason.Trim()));
        }

        return result;
    }

    private static IReadOnlyList<NumberContradictionDto> ParseNumberContradictions(JsonElement root)
    {
        if (!root.TryGetProperty("numberContradictions", out var array)
            || array.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var result = new List<NumberContradictionDto>();
        foreach (var item in array.EnumerateArray())
        {
            var metric = GetString(item, "metric");
            var angle = GetString(item, "factCheckAngle") ?? GetString(item, "angle");
            if (string.IsNullOrWhiteSpace(metric))
            {
                continue;
            }

            var values = new List<SourceValueDto>();
            if (item.TryGetProperty("values", out var valuesArray) && valuesArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var valueItem in valuesArray.EnumerateArray())
                {
                    var sourceName = GetString(valueItem, "sourceName") ?? GetString(valueItem, "source");
                    var value = GetString(valueItem, "value");
                    if (!string.IsNullOrWhiteSpace(sourceName) && !string.IsNullOrWhiteSpace(value))
                    {
                        values.Add(new SourceValueDto(sourceName.Trim(), value.Trim()));
                    }
                }
            }

            if (values.Count < 2)
            {
                continue;
            }

            result.Add(new NumberContradictionDto(
                metric.Trim(),
                values,
                angle?.Trim() ?? string.Empty));
        }

        return result;
    }

    private static IReadOnlyList<SuggestedAngleDto> ParseSuggestedAngles(JsonElement root)
    {
        if (!root.TryGetProperty("suggestedAngles", out var array)
            || array.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var result = new List<SuggestedAngleDto>();
        foreach (var item in array.EnumerateArray())
        {
            var title = GetString(item, "title");
            var rationale = GetString(item, "rationale");
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(rationale))
            {
                continue;
            }

            result.Add(new SuggestedAngleDto(
                title.Trim(),
                rationale.Trim(),
                GetString(item, "angleType") ?? "analysis"));
        }

        return result;
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : property.ToString();
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

internal sealed record ParsedSecondDayResponse(
    IReadOnlyList<ParsedHistoricalParallel> HistoricalParallels,
    IReadOnlyList<StakeholderAngleDto> Stakeholders,
    IReadOnlyList<NumberContradictionDto> NumberContradictions,
    IReadOnlyList<SuggestedAngleDto> SuggestedAngles);

internal sealed record ParsedHistoricalParallel(
    Guid NewsId,
    string ParallelSummary,
    string StructuralSimilarity);

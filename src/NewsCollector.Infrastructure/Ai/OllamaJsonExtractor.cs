using System.Text.RegularExpressions;

namespace NewsCollector.Infrastructure.Ai;

internal static partial class OllamaJsonExtractor
{
    public static string ExtractFirstJsonObject(string rawResponse)
    {
        var trimmed = rawResponse.Trim();
        if (trimmed.Length == 0)
        {
            return trimmed;
        }

        var fenced = CodeFenceRegex().Match(trimmed);
        if (fenced.Success)
        {
            trimmed = fenced.Groups["json"].Value.Trim();
        }

        var start = trimmed.IndexOf('{');
        if (start < 0)
        {
            return trimmed;
        }

        var depth = 0;
        var inString = false;
        var escaped = false;

        for (var i = start; i < trimmed.Length; i++)
        {
            var character = trimmed[i];

            if (inString)
            {
                if (escaped)
                {
                    escaped = false;
                }
                else if (character == '\\')
                {
                    escaped = true;
                }
                else if (character == '"')
                {
                    inString = false;
                }

                continue;
            }

            switch (character)
            {
                case '"':
                    inString = true;
                    break;
                case '{':
                    depth++;
                    break;
                case '}':
                    depth--;
                    if (depth == 0)
                    {
                        return trimmed[start..(i + 1)];
                    }

                    break;
            }
        }

        return trimmed[start..];
    }

    [GeneratedRegex(@"```(?:json)?\s*(?<json>.*?)\s*```", RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex CodeFenceRegex();
}

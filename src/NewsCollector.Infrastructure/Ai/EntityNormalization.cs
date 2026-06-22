using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Infrastructure.Ai;

internal static partial class EntityNormalization
{
    public static string CreateNormalizedKey(string name, NamedEntityType type)
    {
        var slug = Slugify(name);
        return $"{type.ToString().ToLowerInvariant()}:{slug}";
    }

    public static string Slugify(string value)
    {
        var lower = value.Trim().ToLowerInvariant();
        var normalized = lower.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        var ascii = builder.ToString().Normalize(NormalizationForm.FormC);
        ascii = NonWordRegex().Replace(ascii, " ");
        ascii = CollapseSpacesRegex().Replace(ascii.Trim(), "-");

        if (string.IsNullOrWhiteSpace(ascii))
        {
            return "unknown";
        }

        return ascii.Length <= 200 ? ascii : ascii[..200];
    }

    [GeneratedRegex(@"[^\w\s-]", RegexOptions.Compiled)]
    private static partial Regex NonWordRegex();

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex CollapseSpacesRegex();
}

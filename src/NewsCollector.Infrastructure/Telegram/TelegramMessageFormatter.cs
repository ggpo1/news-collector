using System.Net;
using System.Text;
using NewsCollector.Domain.Entities;

namespace NewsCollector.Infrastructure.Telegram;

public static class TelegramMessageFormatter
{
    private const int MaxLength = 3900;

    public static string FormatNews(NewsItem news, string sourceName)
    {
        var body = !string.IsNullOrWhiteSpace(news.Content)
            ? news.Content
            : news.Summary ?? string.Empty;

        body = Truncate(TrimBody(body), MaxLength - 200);

        var builder = new StringBuilder();
        builder.Append("<b>").Append(Escape(news.Title)).Append("</b>\n\n");
        if (!string.IsNullOrWhiteSpace(body))
        {
            builder.Append(Escape(body)).Append("\n\n");
        }

        builder.Append("<i>").Append(Escape(sourceName)).Append("</i>");
        if (!string.IsNullOrWhiteSpace(news.Url))
        {
            builder.Append(" · <a href=\"").Append(EscapeAttribute(news.Url)).Append("\">оригинал</a>");
        }

        return builder.ToString();
    }

    public static string FormatRewrite(NewsRewrite rewrite)
    {
        var body = !string.IsNullOrWhiteSpace(rewrite.Content)
            ? rewrite.Content
            : rewrite.Summary ?? string.Empty;

        body = Truncate(TrimBody(body), MaxLength - 120);

        var builder = new StringBuilder();
        builder.Append("<b>").Append(Escape(rewrite.Title)).Append("</b>\n\n");
        if (!string.IsNullOrWhiteSpace(body))
        {
            builder.Append(Escape(body));
        }

        return builder.ToString();
    }

    public static string MaskToken(string token)
    {
        var trimmed = token.Trim();
        if (trimmed.Length <= 8)
        {
            return "****";
        }

        return $"…{trimmed[^4..]}";
    }

    private static string TrimBody(string value) => value.Trim();

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength].TrimEnd() + "…";
    }

    private static string Escape(string value) =>
        WebUtility.HtmlEncode(value);

    private static string EscapeAttribute(string value) =>
        WebUtility.HtmlEncode(value).Replace("\"", "&quot;");
}

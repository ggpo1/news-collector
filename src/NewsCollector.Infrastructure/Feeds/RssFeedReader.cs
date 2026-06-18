using System.Net;
using System.Security.Cryptography;
using System.ServiceModel.Syndication;
using System.Text;
using System.Text.Json;
using System.Xml;
using Microsoft.Extensions.Logging;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Models;

namespace NewsCollector.Infrastructure.Feeds;

public sealed class RssFeedReader : IRssFeedReader
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    private readonly HttpClient _httpClient;
    private readonly ILogger<RssFeedReader> _logger;

    public RssFeedReader(HttpClient httpClient, ILogger<RssFeedReader> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ParsedFeedItem>> ReadAsync(
        string feedUrl,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching RSS feed from {FeedUrl}", feedUrl);

        await using var stream = await _httpClient.GetStreamAsync(feedUrl, cancellationToken);

        using var reader = XmlReader.Create(stream, new XmlReaderSettings
        {
            Async = true,
            DtdProcessing = DtdProcessing.Prohibit
        });

        var feed = SyndicationFeed.Load(reader);
        var items = new List<ParsedFeedItem>();

        foreach (var item in feed.Items)
        {
            var parsed = MapItem(item);
            if (parsed is not null)
            {
                items.Add(parsed);
            }
        }

        _logger.LogDebug("Parsed {ItemCount} items from {FeedUrl}", items.Count, feedUrl);
        return items;
    }

    private static ParsedFeedItem? MapItem(SyndicationItem item)
    {
        var url = item.Links.FirstOrDefault()?.Uri?.ToString();
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var title = WebUtility.HtmlDecode(item.Title?.Text?.Trim());
        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var summary = ExtractText(item.Summary) ?? ExtractTextFromContent(item.Content);
        var externalId = !string.IsNullOrWhiteSpace(item.Id)
            ? item.Id.Trim()
            : url.Trim();

        var publishedAt = item.PublishDate != default
            ? item.PublishDate.ToUniversalTime()
            : (DateTimeOffset?)null;

        var rawPayload = JsonSerializer.Serialize(new
        {
            item.Id,
            Title = item.Title?.Text,
            Summary = summary,
            Url = url,
            PublishedAt = publishedAt,
            Authors = item.Authors.Select(a => a.Name).ToArray()
        }, JsonOptions);

        return new ParsedFeedItem(externalId, title, summary, url, publishedAt, rawPayload);
    }

    private static string? ExtractText(TextSyndicationContent? content)
    {
        if (content is null || string.IsNullOrWhiteSpace(content.Text))
        {
            return null;
        }

        return WebUtility.HtmlDecode(content.Text.Trim());
    }

    private static string? ExtractTextFromContent(SyndicationContent? content)
    {
        if (content is TextSyndicationContent textContent)
        {
            return ExtractText(textContent);
        }

        return null;
    }

    public static string ComputeContentHash(string title, string? summary)
    {
        var payload = $"{title}|{summary ?? string.Empty}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

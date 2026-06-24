using System.IO.Compression;
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
    private const int MaxFeedBytes = 10 * 1024 * 1024;
    private const int MaxSummaryChars = 4_000;
    private const int MaxRawPayloadChars = 8_000;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    private static int _encodingProviderRegistered;

    private readonly HttpClient _httpClient;
    private readonly ILogger<RssFeedReader> _logger;

    static RssFeedReader()
    {
        EnsureLegacyEncodingsRegistered();
    }

    public RssFeedReader(HttpClient httpClient, ILogger<RssFeedReader> logger)
    {
        EnsureLegacyEncodingsRegistered();
        _httpClient = httpClient;
        _logger = logger;
    }

    public static void EnsureLegacyEncodingsRegistered()
    {
        if (Interlocked.Exchange(ref _encodingProviderRegistered, 1) == 0)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }

    public async Task<IReadOnlyList<ParsedFeedItem>> ReadAsync(
        string feedUrl,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching RSS feed from {FeedUrl}", feedUrl);

        using var response = await _httpClient.GetAsync(
            feedUrl,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var networkStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var rawFeedStream = await ReadLimitedStreamAsync(networkStream, MaxFeedBytes, cancellationToken);

        if (rawFeedStream.Length == 0)
        {
            throw new InvalidOperationException(
                $"RSS feed returned empty body (HTTP {(int)response.StatusCode}). " +
                "The site may require cookies or block automated access.");
        }

        if (IsGzipPayload(rawFeedStream))
        {
            await using var decodedStream = DecompressGzip(rawFeedStream);
            return ParseFeed(decodedStream, feedUrl);
        }

        return ParseFeed(rawFeedStream, feedUrl);
    }

    private IReadOnlyList<ParsedFeedItem> ParseFeed(MemoryStream feedStream, string feedUrl)
    {
        feedStream.Position = 0;

        using var reader = XmlReader.Create(feedStream, new XmlReaderSettings
        {
            Async = false,
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

        _logger.LogInformation(
            "Parsed {ItemCount} items from {FeedUrl} ({FeedBytes} bytes)",
            items.Count,
            feedUrl,
            feedStream.Length);

        return items;
    }

    private static bool IsGzipPayload(MemoryStream stream)
    {
        if (stream.Length < 2)
        {
            return false;
        }

        stream.Position = 0;
        var header = stream.ReadByte();
        var second = stream.ReadByte();
        stream.Position = 0;
        return header == 0x1F && second == 0x8B;
    }

    private static MemoryStream DecompressGzip(MemoryStream raw)
    {
        raw.Position = 0;
        var decompressed = new MemoryStream();
        using (var gzip = new GZipStream(raw, CompressionMode.Decompress, leaveOpen: true))
        {
            gzip.CopyTo(decompressed);
        }

        decompressed.Position = 0;
        return decompressed;
    }

    private static async Task<MemoryStream> ReadLimitedStreamAsync(
        Stream stream,
        int maxBytes,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[16 * 1024];
        var memory = new MemoryStream();

        while (true)
        {
            var read = await stream.ReadAsync(buffer, cancellationToken);
            if (read == 0)
            {
                break;
            }

            var nextLength = memory.Length + read;
            if (nextLength > maxBytes)
            {
                throw new InvalidOperationException($"RSS feed exceeds {maxBytes} bytes.");
            }

            memory.Write(buffer, 0, read);
        }

        memory.Position = 0;
        return memory;
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

        var summary = Truncate(
            ExtractText(item.Summary) ?? ExtractTextFromContent(item.Content),
            MaxSummaryChars);
        var externalId = !string.IsNullOrWhiteSpace(item.Id)
            ? item.Id.Trim()
            : url.Trim();

        var publishedAt = item.PublishDate != default
            ? item.PublishDate.ToUniversalTime()
            : (DateTimeOffset?)null;

        var rawPayload = Truncate(JsonSerializer.Serialize(new
        {
            item.Id,
            Title = item.Title?.Text,
            Summary = summary,
            Url = url,
            PublishedAt = publishedAt,
            Authors = item.Authors.Select(a => a.Name).ToArray()
        }, JsonOptions), MaxRawPayloadChars) ?? "{}";

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

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
    }

    public static string ComputeContentHash(string title, string? summary)
    {
        var payload = $"{title}|{summary ?? string.Empty}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

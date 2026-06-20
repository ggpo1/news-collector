using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Options;
using NewsCollector.Domain.Entities;
using NewsCollector.Infrastructure.Ai;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class NewsCategorizationService : INewsCategorizationService
{
    private const string SystemPrompt =
        """
        Ты классификатор новостей. Определи одну наиболее подходящую категорию для новости.
        Используй только slug из предложенного списка. Если ни одна категория не подходит — верни slug "other".

        Верни ТОЛЬКО валидный JSON:
        {"categorySlug":"slug-из-списка"}
        """;

    private readonly NewsCollectorDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OllamaOptions _ollamaOptions;
    private readonly NewsCategorizerOptions _options;
    private readonly ILogger<NewsCategorizationService> _logger;

    public NewsCategorizationService(
        NewsCollectorDbContext db,
        IHttpClientFactory httpClientFactory,
        IOptions<OllamaOptions> ollamaOptions,
        IOptions<NewsCategorizerOptions> options,
        ILogger<NewsCategorizationService> logger)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _ollamaOptions = ollamaOptions.Value;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> CategorizePendingNewsAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);

        if (categories.Count == 0)
        {
            _logger.LogWarning("No active categories configured");
            return 0;
        }

        var slugToId = categories.ToDictionary(c => c.Slug, c => c.Id, StringComparer.OrdinalIgnoreCase);
        var fallbackCategoryId = slugToId.GetValueOrDefault("other", categories[0].Id);

        var pendingItems = await _db.NewsItems
            .Where(n => n.CategoryId == null)
            .OrderByDescending(n => n.FetchedAt)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        if (pendingItems.Count == 0)
        {
            return 0;
        }

        var categorizedCount = 0;

        foreach (var item in pendingItems)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var slug = await ClassifyAsync(item, categories, cancellationToken);
                if (!slugToId.TryGetValue(slug, out var categoryId))
                {
                    _logger.LogWarning(
                        "Unknown category slug '{CategorySlug}' for news {NewsId}, using fallback",
                        slug,
                        item.Id);
                    categoryId = fallbackCategoryId;
                }

                item.CategoryId = categoryId;
                categorizedCount++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Failed to categorize news {NewsId}, using fallback category", item.Id);
                item.CategoryId = fallbackCategoryId;
                categorizedCount++;
            }

            if (_options.DelayBetweenItemsMs > 0)
            {
                await Task.Delay(_options.DelayBetweenItemsMs, cancellationToken);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        if (categorizedCount > 0)
        {
            _logger.LogInformation("Categorized {Count} news items", categorizedCount);
        }

        return categorizedCount;
    }

    private async Task<string> ClassifyAsync(
        NewsItem item,
        IReadOnlyList<Category> categories,
        CancellationToken cancellationToken)
    {
        var userMessage = BuildUserMessage(item, categories);
        var rawResponse = await ChatAsync(item.Id, userMessage, cancellationToken);
        return OllamaCategoryResponseParser.ParseSlug(rawResponse);
    }

    private static string BuildUserMessage(NewsItem item, IReadOnlyList<Category> categories)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Доступные категории:");

        foreach (var category in categories)
        {
            builder.Append("- ");
            builder.Append(category.Slug);
            builder.Append(": ");
            builder.Append(category.Name);

            if (!string.IsNullOrWhiteSpace(category.Description))
            {
                builder.Append(" — ");
                builder.Append(category.Description);
            }

            builder.AppendLine();
        }

        builder.AppendLine();
        builder.AppendLine($"Заголовок: {item.Title}");

        if (!string.IsNullOrWhiteSpace(item.Summary))
        {
            builder.AppendLine($"Краткое описание: {item.Summary.Trim()}");
        }

        var body = !string.IsNullOrWhiteSpace(item.Content)
            ? item.Content.Trim()
            : item.Summary?.Trim();

        if (!string.IsNullOrWhiteSpace(body))
        {
            var excerpt = body.Length > 2_000 ? body[..2_000] : body;
            builder.AppendLine("Текст:");
            builder.AppendLine(excerpt);
        }

        return builder.ToString();
    }

    private async Task<string> ChatAsync(
        Guid newsId,
        string userMessage,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("Ollama");

        var request = new OllamaChatRequest(
            _ollamaOptions.Model,
            [
                new OllamaChatMessage("system", SystemPrompt),
                new OllamaChatMessage("user", userMessage)
            ],
            Stream: false,
            Format: "json",
            KeepAlive: "30m");

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_ollamaOptions.TimeoutSeconds));

        var response = await client.PostAsJsonAsync("/api/chat", request, timeoutCts.Token);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Ollama returned {(int)response.StatusCode}: {body}");
        }

        var payload = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken);
        var content = payload?.Message?.Content;
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Ollama returned an empty categorization response.");
        }

        _logger.LogDebug("Ollama categorized news {NewsId}: {Response}", newsId, content);
        return content;
    }

    private sealed record OllamaChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] OllamaChatMessage[] Messages,
        [property: JsonPropertyName("stream")] bool Stream,
        [property: JsonPropertyName("format")] string Format,
        [property: JsonPropertyName("keep_alive")] string KeepAlive);

    private sealed record OllamaChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record OllamaChatResponse(
        [property: JsonPropertyName("message")] OllamaChatMessage? Message);
}

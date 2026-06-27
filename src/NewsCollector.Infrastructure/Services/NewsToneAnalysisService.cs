using System.Net.Http.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Options;
using NewsCollector.Domain.Entities;
using NewsCollector.Infrastructure.Ai;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class NewsToneAnalysisService : INewsToneAnalysisService
{
    private const string SystemPrompt =
        """
        Ты аналитик тональности новостей. Оцени эмоциональную окраску текста по шкале от -1 до +1:
        -1 = резко негативная (катастрофа, конфликт, преступление, скандал)
         0 = нейтральная (факты без явной оценки)
        +1 = резко позитивная (успех, достижение, праздник, помощь)

        Учитывай заголовок и текст. Верни ТОЛЬКО валидный JSON:
        {"toneCoefficient":0.0}
        """;

    private readonly NewsCollectorDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OllamaOptions _ollamaOptions;
    private readonly NewsToneAnalyzerOptions _options;
    private readonly ILogger<NewsToneAnalysisService> _logger;

    public NewsToneAnalysisService(
        NewsCollectorDbContext db,
        IHttpClientFactory httpClientFactory,
        IOptions<OllamaOptions> ollamaOptions,
        IOptions<NewsToneAnalyzerOptions> options,
        ILogger<NewsToneAnalysisService> logger)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _ollamaOptions = ollamaOptions.Value;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> AnalyzePendingNewsAsync(CancellationToken cancellationToken = default)
    {
        var pendingItems = await _db.NewsItems
            .Where(n => n.ToneCoefficient == null)
            .OrderByDescending(n => n.PublishedAt ?? n.FetchedAt)
            .ThenByDescending(n => n.FetchedAt)
            .ThenByDescending(n => n.CreatedAt)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        if (pendingItems.Count == 0)
        {
            return 0;
        }

        var analyzedCount = 0;
        var analyzedAt = DateTimeOffset.UtcNow;

        foreach (var item in pendingItems)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                item.ToneCoefficient = await AnalyzeAsync(item, cancellationToken);
                item.ToneAnalyzedAt = analyzedAt;
                analyzedCount++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Failed to analyze tone for news {NewsId}", item.Id);
            }

            if (_options.DelayBetweenItemsMs > 0)
            {
                await Task.Delay(_options.DelayBetweenItemsMs, cancellationToken);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        if (analyzedCount > 0)
        {
            _logger.LogInformation("Analyzed tone for {Count} news items", analyzedCount);
        }

        return analyzedCount;
    }

    private async Task<decimal> AnalyzeAsync(NewsItem item, CancellationToken cancellationToken)
    {
        var userMessage = BuildUserMessage(item);
        var rawResponse = await ChatAsync(item.Id, userMessage, cancellationToken);
        return OllamaToneResponseParser.ParseCoefficient(rawResponse);
    }

    private static string BuildUserMessage(NewsItem item)
    {
        var builder = new StringBuilder();
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

        var request = OllamaChatHelper.CreateRequest(
            _ollamaOptions,
            [
                new OllamaChatHelper.OllamaChatMessage("system", SystemPrompt),
                new OllamaChatHelper.OllamaChatMessage("user", userMessage),
            ],
            format: "json");

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_ollamaOptions.TimeoutSeconds));

        var response = await client.PostAsJsonAsync("/api/chat", request, timeoutCts.Token);
        var content = await OllamaChatHelper.ReadChatContentAsync(response, _ollamaOptions, cancellationToken);
        _logger.LogDebug("Ollama tone analysis for news {NewsId}: {Response}", newsId, content);
        return content;
    }
}

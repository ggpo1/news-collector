using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Application.Options;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Ai;

public sealed class OllamaAiNewsRewriteService : IAiNewsRewriteService
{
    private const string SystemPrompt =
        """
        Ты профессиональный редактор новостей.
        Перепиши новость своими словами: сохрани факты и смысл, улучши читаемость, используй нейтральный информационный стиль на русском языке.
        Не добавляй факты, которых нет в исходнике. Не используй markdown.

        Верни ТОЛЬКО валидный JSON в формате:
        {"title":"...","summary":"краткое описание в 1-2 предложения","content":"полный переписанный текст"}
        """;

    private readonly NewsCollectorDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OllamaOptions _options;
    private readonly ILogger<OllamaAiNewsRewriteService> _logger;

    public OllamaAiNewsRewriteService(
        NewsCollectorDbContext db,
        IHttpClientFactory httpClientFactory,
        IOptions<OllamaOptions> options,
        ILogger<OllamaAiNewsRewriteService> logger)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AiNewsRewriteResultDto?> RewriteAsync(
        Guid newsId,
        CancellationToken cancellationToken = default)
    {
        var news = await _db.NewsItems
            .AsNoTracking()
            .Where(n => n.Id == newsId)
            .Select(n => new NewsRewriteSource(
                n.Title,
                n.Summary,
                n.Content,
                n.Source.Name))
            .FirstOrDefaultAsync(cancellationToken);

        if (news is null)
        {
            return null;
        }

        var sourceText = BuildSourceText(news);
        if (string.IsNullOrWhiteSpace(sourceText))
        {
            throw new InvalidOperationException("У новости нет текста для переписывания. Сначала загрузите полный текст.");
        }

        var userMessage = BuildUserMessage(news, sourceText);
        var rawResponse = await ChatAsync(newsId, userMessage);

        try
        {
            return OllamaRewriteResponseParser.Parse(rawResponse);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse Ollama rewrite response: {Response}", rawResponse);
            throw new InvalidOperationException("Не удалось разобрать ответ модели. Попробуйте ещё раз.", ex);
        }
    }

    private async Task<string> ChatAsync(Guid newsId, string userMessage)
    {
        var client = _httpClientFactory.CreateClient("Ollama");

        var request = new OllamaChatRequest(
            _options.Model,
            [
                new OllamaChatMessage("system", SystemPrompt),
                new OllamaChatMessage("user", userMessage)
            ],
            Stream: false,
            Format: "json",
            KeepAlive: "30m");

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Ollama /api/chat started: NewsId={NewsId}, Model={Model}, BaseUrl={BaseUrl}, TimeoutSeconds={TimeoutSeconds}, UserMessageLength={UserMessageLength}",
            newsId,
            _options.Model,
            _options.BaseUrl,
            _options.TimeoutSeconds,
            userMessage.Length);

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync("/api/chat", request, timeoutCts.Token);
        }
        catch (TaskCanceledException ex) when (!timeoutCts.IsCancellationRequested)
        {
            _logger.LogError(
                ex,
                "Ollama request canceled externally after {ElapsedSeconds}s (NewsId={NewsId})",
                stopwatch.Elapsed.TotalSeconds,
                newsId);
            throw new InvalidOperationException(
                "Запрос к Ollama был прерван (nginx/браузер). Проверьте proxy_read_timeout и OLLAMA_TIMEOUT_SECONDS.",
                ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(
                ex,
                "Ollama request timed out after {TimeoutSeconds}s, elapsed {ElapsedSeconds}s (NewsId={NewsId})",
                _options.TimeoutSeconds,
                stopwatch.Elapsed.TotalSeconds,
                newsId);
            throw new InvalidOperationException(
                $"Ollama не ответила за {_options.TimeoutSeconds} секунд. Увеличьте OLLAMA_TIMEOUT_SECONDS (рекомендуется 1800+) или выберите модель полегче.",
                ex);
        }
        catch (Exception ex) when (ex is HttpRequestException)
        {
            _logger.LogError(
                ex,
                "Ollama connection failed after {ElapsedSeconds}s at {BaseUrl} (NewsId={NewsId})",
                stopwatch.Elapsed.TotalSeconds,
                _options.BaseUrl,
                newsId);
            throw new InvalidOperationException(
                $"Не удалось подключиться к Ollama ({_options.BaseUrl}). " +
                "В Docker используйте OLLAMA_BASE_URL=http://ollama:11434.",
                ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(timeoutCts.Token);
            _logger.LogWarning(
                "Ollama returned {StatusCode} after {ElapsedSeconds}s (NewsId={NewsId}): {Body}",
                (int)response.StatusCode,
                stopwatch.Elapsed.TotalSeconds,
                newsId,
                body);

            var timeoutHint = response.StatusCode == System.Net.HttpStatusCode.InternalServerError
                ? " Часто это обрыв по таймауту API (OLLAMA_TIMEOUT_SECONDS, nginx proxy_read_timeout)."
                : string.Empty;

            throw new InvalidOperationException(
                $"Ollama вернула ошибку {(int)response.StatusCode}.{timeoutHint} Модель: '{_options.Model}'.");
        }

        var payload = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(timeoutCts.Token);
        var content = payload?.Message?.Content;
        if (string.IsNullOrWhiteSpace(content))
        {
            _logger.LogWarning(
                "Ollama returned empty message after {ElapsedSeconds}s (NewsId={NewsId})",
                stopwatch.Elapsed.TotalSeconds,
                newsId);
            throw new InvalidOperationException("Ollama вернула пустой ответ.");
        }

        _logger.LogInformation(
            "Ollama /api/chat completed in {ElapsedSeconds}s (NewsId={NewsId}, ResponseLength={ResponseLength})",
            stopwatch.Elapsed.TotalSeconds,
            newsId,
            content.Length);

        return content;
    }

    private static string BuildSourceText(NewsRewriteSource news)
    {
        if (!string.IsNullOrWhiteSpace(news.Content))
        {
            return news.Content.Trim();
        }

        if (!string.IsNullOrWhiteSpace(news.Summary))
        {
            return news.Summary.Trim();
        }

        return news.Title.Trim();
    }

    private static string BuildUserMessage(NewsRewriteSource news, string sourceText)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Источник: {news.SourceName}");
        builder.AppendLine($"Заголовок: {news.Title}");

        if (!string.IsNullOrWhiteSpace(news.Summary))
        {
            builder.AppendLine($"Краткое описание: {news.Summary.Trim()}");
        }

        builder.AppendLine("Текст новости:");
        builder.AppendLine(sourceText);

        return builder.ToString();
    }

    private sealed record NewsRewriteSource(
        string Title,
        string? Summary,
        string? Content,
        string SourceName);

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

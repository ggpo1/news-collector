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
        var rawResponse = await ChatAsync(userMessage);

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

    private async Task<string> ChatAsync(string userMessage)
    {
        var client = _httpClientFactory.CreateClient("Ollama");

        var request = new OllamaChatRequest(
            _options.Model,
            [
                new OllamaChatMessage("system", SystemPrompt),
                new OllamaChatMessage("user", userMessage)
            ],
            Stream: false,
            Format: "json");

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync("/api/chat", request, timeoutCts.Token);
        }
        catch (TaskCanceledException ex) when (!timeoutCts.IsCancellationRequested)
        {
            _logger.LogError(ex, "Ollama request was canceled before completion at {BaseUrl}", _options.BaseUrl);
            throw new InvalidOperationException(
                "Запрос к Ollama был прерван. Проверьте таймауты nginx и OLLAMA_TIMEOUT_SECONDS.",
                ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Ollama request timed out after {TimeoutSeconds}s at {BaseUrl}", _options.TimeoutSeconds, _options.BaseUrl);
            throw new InvalidOperationException(
                $"Ollama не ответила за {_options.TimeoutSeconds} секунд. Увеличьте OLLAMA_TIMEOUT_SECONDS или выберите модель полегче.",
                ex);
        }
        catch (Exception ex) when (ex is HttpRequestException)
        {
            _logger.LogError(ex, "Ollama request failed at {BaseUrl}", _options.BaseUrl);
            throw new InvalidOperationException(
                $"Не удалось подключиться к Ollama ({_options.BaseUrl}). " +
                "В Docker используйте OLLAMA_BASE_URL=http://ollama:11434 или запустите Ollama на хосте с OLLAMA_HOST=0.0.0.0:11434.",
                ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(timeoutCts.Token);
            _logger.LogWarning(
                "Ollama returned {StatusCode}: {Body}",
                (int)response.StatusCode,
                body);

            throw new InvalidOperationException(
                $"Ollama вернула ошибку {(int)response.StatusCode}. Проверьте, что модель '{_options.Model}' установлена.");
        }

        var payload = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(timeoutCts.Token);
        var content = payload?.Message?.Content;
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Ollama вернула пустой ответ.");
        }

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
        [property: JsonPropertyName("format")] string Format);

    private sealed record OllamaChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record OllamaChatResponse(
        [property: JsonPropertyName("message")] OllamaChatMessage? Message);
}

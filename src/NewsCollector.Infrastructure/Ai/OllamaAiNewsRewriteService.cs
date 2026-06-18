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

        var prompt = BuildPrompt(news, sourceText);
        var rawResponse = await GenerateAsync(prompt, cancellationToken);

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

    private async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("Ollama");

        var request = new OllamaGenerateRequest(
            _options.Model,
            prompt,
            Stream: false,
            Format: "json");

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync("/api/generate", request, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Ollama request failed at {BaseUrl}", _options.BaseUrl);
            throw new InvalidOperationException(
                $"Не удалось подключиться к Ollama ({_options.BaseUrl}). Убедитесь, что сервис запущен.",
                ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Ollama returned {StatusCode}: {Body}",
                (int)response.StatusCode,
                body);

            throw new InvalidOperationException(
                $"Ollama вернула ошибку {(int)response.StatusCode}. Проверьте, что модель '{_options.Model}' установлена.");
        }

        var payload = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken);
        if (string.IsNullOrWhiteSpace(payload?.Response))
        {
            throw new InvalidOperationException("Ollama вернула пустой ответ.");
        }

        return payload.Response;
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

    private static string BuildPrompt(NewsRewriteSource news, string sourceText)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Ты профессиональный редактор новостей.");
        builder.AppendLine("Перепиши новость своими словами: сохрани факты и смысл, улучши читаемость, используй нейтральный информационный стиль на русском языке.");
        builder.AppendLine("Не добавляй факты, которых нет в исходнике. Не используй markdown.");
        builder.AppendLine();
        builder.AppendLine("Верни ТОЛЬКО валидный JSON в формате:");
        builder.AppendLine("{\"title\":\"...\",\"summary\":\"краткое описание в 1-2 предложения\",\"content\":\"полный переписанный текст\"}");
        builder.AppendLine();
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

    private sealed record OllamaGenerateRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("prompt")] string Prompt,
        [property: JsonPropertyName("stream")] bool Stream,
        [property: JsonPropertyName("format")] string Format);

    private sealed record OllamaGenerateResponse(
        [property: JsonPropertyName("response")] string Response);
}

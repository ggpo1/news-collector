using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Application.Options;
using NewsCollector.Infrastructure.Ai;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class SecondDayAngleService : ISecondDayAngleService
{
    private const string SystemPrompt =
        """
        Ты шеф-редактор, который готовит материалы «на второй день» после инфоповода.
        Твоя задача — найти неочевидные углы, а не пересказать факты.

        Правила:
        - Исторические параллели: сравнивай СТРУКТУРУ ситуации (кто, конфликт интересов, последствия), а не ключевые слова.
        - Пострадавшие/выгодоприобретатели: кто теряет/выигрывает от события и почему.
        - Противоречия в цифрах: только если в разных источниках реально разные числа по одной метрике.
        - suggestedAngles: ровно 5–7 готовых тем для статей с цепляющим заголовком и кратким обоснованием.

        Используй только newsId из списка кандидатов для historicalParallels.
        role для stakeholders: affected | beneficiary | neutral

        Верни ТОЛЬКО валидный JSON:
        {
          "historicalParallels":[
            {"newsId":"uuid","parallelSummary":"...","structuralSimilarity":"..."}
          ],
          "stakeholders":[
            {"name":"...","type":"company","role":"affected","reason":"..."}
          ],
          "numberContradictions":[
            {"metric":"ущерб","values":[{"sourceName":"...","value":"..."}],"factCheckAngle":"..."}
          ],
          "suggestedAngles":[
            {"title":"...","rationale":"...","angleType":"historical|stakeholder|factcheck|consequence|profile|investigation"}
          ]
        }
        """;

    private readonly NewsCollectorDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OllamaOptions _ollamaOptions;
    private readonly ILogger<SecondDayAngleService> _logger;

    public SecondDayAngleService(
        NewsCollectorDbContext db,
        IHttpClientFactory httpClientFactory,
        IOptions<OllamaOptions> ollamaOptions,
        ILogger<SecondDayAngleService> logger)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _ollamaOptions = ollamaOptions.Value;
        _logger = logger;
    }

    public async Task<SecondDayAnglesDto?> GenerateAsync(
        Guid newsId,
        CancellationToken cancellationToken = default)
    {
        var builder = new SecondDayAngleContextBuilder(_db);
        var context = await builder.BuildAsync(newsId, cancellationToken);
        if (context is null)
        {
            return null;
        }

        var userMessage = BuildUserMessage(context);
        var rawResponse = await ChatAsync(newsId, userMessage, cancellationToken);
        var parsed = OllamaSecondDayResponseParser.Parse(rawResponse);

        var candidateById = context.HistoricalCandidates.ToDictionary(c => c.NewsId);
        var historicalParallels = parsed.HistoricalParallels
            .Where(p => candidateById.ContainsKey(p.NewsId))
            .Select(p =>
            {
                var candidate = candidateById[p.NewsId];
                return new HistoricalParallelDto(
                    candidate.NewsId,
                    candidate.Title,
                    candidate.SourceName,
                    candidate.PublishedAt,
                    p.ParallelSummary,
                    p.StructuralSimilarity);
            })
            .ToList();

        _logger.LogInformation(
            "Second-day angles for news {NewsId}: {Parallels} parallels, {Stakeholders} stakeholders, {Contradictions} contradictions, {Angles} angles",
            newsId,
            historicalParallels.Count,
            parsed.Stakeholders.Count,
            parsed.NumberContradictions.Count,
            parsed.SuggestedAngles.Count);

        return new SecondDayAnglesDto(
            context.Source.Id,
            context.Source.Title,
            historicalParallels,
            parsed.Stakeholders,
            parsed.NumberContradictions,
            parsed.SuggestedAngles);
    }

    private static string BuildUserMessage(SecondDayAngleContext context)
    {
        var source = context.Source;
        var builder = new StringBuilder();

        builder.AppendLine("=== ИНФОПОВОД (событие «день 1») ===");
        builder.AppendLine($"ID: {source.Id}");
        builder.AppendLine($"Источник: {source.Source.Name}");
        builder.AppendLine($"Категория: {source.Category?.Name ?? "не указана"}");
        builder.AppendLine($"Заголовок: {source.Title}");
        AppendExcerpt(builder, source.Summary, source.Content, 2_500);

        if (context.SourceEntities.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("=== СУЩНОСТИ В ПОВОДЕ ===");
            foreach (var entity in context.SourceEntities)
            {
                builder.Append("- ");
                builder.Append(entity.Name);
                builder.Append(" (");
                builder.Append(entity.Type);
                builder.AppendLine(")");
            }
        }

        if (context.CoMentionEntities.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("=== СВЯЗАННЫЕ СУЩНОСТИ (совместные упоминания) ===");
            foreach (var entity in context.CoMentionEntities)
            {
                builder.Append("- ");
                builder.Append(entity.Name);
                builder.Append(" (");
                builder.Append(entity.Type);
                builder.Append(", co-mention ");
                builder.Append(entity.CoMentionCount);
                builder.AppendLine(")");
            }
        }

        if (context.HistoricalCandidates.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("=== КАНДИДАТЫ ИЗ АРХИВА (для исторических параллелей) ===");
            foreach (var candidate in context.HistoricalCandidates)
            {
                builder.AppendLine($"--- newsId: {candidate.NewsId} ---");
                builder.AppendLine($"Дата: {candidate.PublishedAt:yyyy-MM-dd}");
                builder.AppendLine($"Источник: {candidate.SourceName}");
                builder.AppendLine($"Общих сущностей: {candidate.SharedEntityCount}");
                builder.AppendLine($"Заголовок: {candidate.Title}");
                AppendExcerpt(builder, candidate.Summary, candidate.Content, 800);
                builder.AppendLine();
            }
        }

        if (context.SameTopicCoverage.Count > 0)
        {
            builder.AppendLine("=== ДРУГИЕ ИСТОЧНИКИ О ТОМ ЖЕ СОБЫТИИ (для сверки цифр) ===");
            foreach (var coverage in context.SameTopicCoverage)
            {
                builder.AppendLine($"--- {coverage.SourceName} ---");
                builder.AppendLine($"Заголовок: {coverage.Title}");
                AppendExcerpt(builder, coverage.Summary, coverage.Content, 1_000);
                builder.AppendLine();
            }
        }

        return builder.ToString();
    }

    private static void AppendExcerpt(StringBuilder builder, string? summary, string? content, int maxLength)
    {
        var text = !string.IsNullOrWhiteSpace(content) ? content.Trim() : summary?.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var excerpt = text.Length > maxLength ? text[..maxLength] : text;
        builder.AppendLine("Текст:");
        builder.AppendLine(excerpt);
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
            throw new InvalidOperationException("Ollama returned an empty second-day angles response.");
        }

        _logger.LogDebug("Ollama second-day angles for news {NewsId}: {Length} chars", newsId, content.Length);
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

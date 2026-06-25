using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Application.Options;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Infrastructure.Services;

public sealed class EditorialBriefGenerator
{
    private const string SystemPrompt =
        """
        Ты главный редактор новостного агрегатора. Собери редакционный бриф на русском языке в формате Markdown.

        Структура (строго соблюдай заголовки):
        ## Главные темы
        5–7 тем с кратким описанием, числом источников и ключевыми материалами.

        ## Что нового
        Что появилось с прошлого брифа / за окно — новые материалы, развитие тем.

        ## Уже в Telegram
        Что уже отправлено в каналы — чтобы редакция не дублировала выпуск.

        ## Дыры в покрытии
        Темы только в RU или только в западных источниках — где стоит добрать материалы.

        ## На что обратить внимание
        Всплески сущностей, эмоциональные поводы, темы в работе (если есть в данных).

        Правила:
        - Пиши конкретно, без воды и без выдуманных фактов.
        - Используй только данные из контекста.
        - Если раздел пуст — напиши «Пока нет данных» одной строкой.
        - Не добавляй JSON, только Markdown.
        """;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OllamaOptions _ollamaOptions;
    private readonly ILogger<EditorialBriefGenerator> _logger;

    public EditorialBriefGenerator(
        IHttpClientFactory httpClientFactory,
        IOptions<OllamaOptions> ollamaOptions,
        ILogger<EditorialBriefGenerator> logger)
    {
        _httpClientFactory = httpClientFactory;
        _ollamaOptions = ollamaOptions.Value;
        _logger = logger;
    }

    public async Task<string> GenerateMarkdownAsync(
        EditorialBriefBuildContext context,
        CancellationToken cancellationToken = default)
    {
        var userMessage = BuildUserMessage(context);
        var markdown = await ChatAsync(userMessage, cancellationToken);

        _logger.LogInformation(
            "Generated editorial brief ({Period}) — {Length} chars",
            context.Period,
            markdown.Length);

        return markdown.Trim();
    }

    private static string BuildUserMessage(EditorialBriefBuildContext context)
    {
        var builder = new StringBuilder();
        var periodLabel = context.Period == EditorialBriefPeriod.Morning ? "Утренний" : "Вечерний";

        builder.AppendLine($"=== {periodLabel} бриф ===");
        builder.AppendLine($"Окно: {context.WindowStart:yyyy-MM-dd HH:mm} — {context.WindowEnd:yyyy-MM-dd HH:mm} UTC");
        if (context.PreviousBriefAt.HasValue)
        {
            builder.AppendLine($"Предыдущий бриф: {context.PreviousBriefAt:yyyy-MM-dd HH:mm} UTC");
        }

        builder.AppendLine();
        builder.AppendLine("=== РАЗВИВАЮЩИЕСЯ ТЕМЫ (дашборд) ===");
        if (context.Dashboard.DevelopingTopics.Count == 0)
        {
            builder.AppendLine("Нет тем с 3+ источниками.");
        }
        else
        {
            foreach (var topic in context.Dashboard.DevelopingTopics)
            {
                builder.AppendLine($"- {topic.Headline}");
                builder.AppendLine($"  Источники ({topic.SourceCount}): {string.Join(", ", topic.SourceNames)}");
                builder.AppendLine($"  Материалов: {topic.ArticleCount}");
                builder.AppendLine($"  Лучший: [{topic.PrimaryArticle.SourceName}] {topic.PrimaryArticle.Title}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("=== АКТИВНЫЕ ТЕМЫ (Story) ===");
        if (context.ActiveStories.Count == 0)
        {
            builder.AppendLine("Нет активных Story за окно.");
        }
        else
        {
            foreach (var story in context.ActiveStories)
            {
                builder.AppendLine($"- [{story.Status}] {story.Title}");
                builder.AppendLine($"  Материалов: {story.ArticleCount}, источников: {story.SourceCount}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("=== НОВЫЕ МАТЕРИАЛЫ С ПРОШЛОГО БРИФА ===");
        if (context.NewSincePrevious.Count == 0)
        {
            builder.AppendLine("Нет данных или это первый бриф.");
        }
        else
        {
            foreach (var item in context.NewSincePrevious.Take(25))
            {
                builder.AppendLine($"- [{item.SourceName}] {item.Title} ({item.PublishedAt:yyyy-MM-dd HH:mm})");
            }
        }

        builder.AppendLine();
        builder.AppendLine("=== УЖЕ ОТПРАВЛЕНО В TELEGRAM ===");
        if (context.TelegramSent.Count == 0)
        {
            builder.AppendLine("За окно ничего не отправлялось.");
        }
        else
        {
            foreach (var item in context.TelegramSent)
            {
                var title = item.NewsItemId.HasValue && context.NewsTitles.TryGetValue(item.NewsItemId.Value, out var newsTitle)
                    ? newsTitle
                    : Truncate(item.MessageText, 120);

                builder.AppendLine($"- [{item.ChannelName}] {title} ({item.SentAt:yyyy-MM-dd HH:mm})");
            }
        }

        builder.AppendLine();
        builder.AppendLine("=== ДЫРЫ В ПОКРЫТИИ ===");
        if (context.CoverageGaps.Count == 0)
        {
            builder.AppendLine("Явных дыр не обнаружено.");
        }
        else
        {
            foreach (var gap in context.CoverageGaps)
            {
                builder.AppendLine($"- {gap.Description}: {gap.Headline}");
                builder.AppendLine($"  Источники: {string.Join(", ", gap.SourceNames)}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("=== ВСПЛЕСКИ СУЩНОСТЕЙ ===");
        foreach (var spike in context.Dashboard.EntitySpikes.Take(5))
        {
            builder.AppendLine($"- {spike.EntityName} ({spike.EntityType}): ×{spike.SpikeRatio:F1}, {spike.MentionsInWindow} упоминаний");
        }

        builder.AppendLine();
        builder.AppendLine("=== ЭМОЦИОНАЛЬНЫЕ ПОВОДЫ ===");
        foreach (var highlight in context.Dashboard.ToneHighlights.Take(5))
        {
            builder.AppendLine($"- [{highlight.ToneLabel}] {highlight.News.Title} ({highlight.News.SourceName})");
        }

        return builder.ToString();
    }

    private async Task<string> ChatAsync(string userMessage, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("Ollama");

        var request = new OllamaChatRequest(
            _ollamaOptions.Model,
            [
                new OllamaChatMessage("system", SystemPrompt),
                new OllamaChatMessage("user", userMessage)
            ],
            Stream: false,
            KeepAlive: "30m");

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_ollamaOptions.TimeoutSeconds));

        var response = await client.PostAsJsonAsync("/api/chat", request, timeoutCts.Token);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Ollama returned {(int)response.StatusCode}: {body}");
        }

        var payload = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken);
        var content = payload?.Message?.Content;
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Ollama returned an empty editorial brief.");
        }

        return content;
    }

    private static string Truncate(string text, int maxLength)
    {
        var trimmed = text.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength] + "…";
    }

    private sealed record OllamaChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] OllamaChatMessage[] Messages,
        [property: JsonPropertyName("stream")] bool Stream,
        [property: JsonPropertyName("keep_alive")] string KeepAlive);

    private sealed record OllamaChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record OllamaChatResponse(
        [property: JsonPropertyName("message")] OllamaChatMessage? Message);
}

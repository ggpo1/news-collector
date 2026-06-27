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

public sealed class NewsEntityExtractionService : INewsEntityExtractionService
{
    private const string SystemPrompt =
        """
        Ты извлекаешь именованные сущности из новостей для построения семантической карты связей.
        Найди персоны, компании, страны, организации, локации и значимые события, упомянутые в тексте.
        Не включай общие слова, жанры, абстрактные понятия и источники СМИ.
        Для каждой сущности укажи каноническое имя (как в энциклопедии) и тип.

        Допустимые типы: person, company, country, organization, location, event, other.

        Верни ТОЛЬКО валидный JSON:
        {"entities":[{"name":"Владимир Путин","type":"person"},{"name":"Россия","type":"country"},{"name":"Apple","type":"company"}]}
        """;

    private readonly NewsCollectorDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OllamaOptions _ollamaOptions;
    private readonly NewsEntityExtractorOptions _options;
    private readonly ILogger<NewsEntityExtractionService> _logger;

    public NewsEntityExtractionService(
        NewsCollectorDbContext db,
        IHttpClientFactory httpClientFactory,
        IOptions<OllamaOptions> ollamaOptions,
        IOptions<NewsEntityExtractorOptions> options,
        ILogger<NewsEntityExtractionService> logger)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _ollamaOptions = ollamaOptions.Value;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> ExtractPendingNewsAsync(CancellationToken cancellationToken = default)
    {
        var pendingItems = await _db.NewsItems
            .Where(n => n.EntitiesExtractedAt == null)
            .Where(n => n.Content != null || n.Summary != null)
            .OrderByDescending(n => n.PublishedAt ?? n.FetchedAt)
            .ThenByDescending(n => n.FetchedAt)
            .ThenByDescending(n => n.CreatedAt)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        if (pendingItems.Count == 0)
        {
            return 0;
        }

        var processedCount = 0;
        var extractedAt = DateTimeOffset.UtcNow;
        var entityCache = new Dictionary<string, NamedEntity>(StringComparer.Ordinal);

        foreach (var item in pendingItems)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var rawResponse = await ChatAsync(item.Id, BuildUserMessage(item), cancellationToken);
                var extracted = OllamaEntityResponseParser.ParseEntities(rawResponse, _options.MaxEntitiesPerNews);
                await PersistMentionsAsync(item, extracted, extractedAt, entityCache, cancellationToken);
                item.EntitiesExtractedAt = extractedAt;
                processedCount++;

                _logger.LogInformation(
                    "Extracted {EntityCount} entities for news {NewsId}",
                    extracted.Count,
                    item.Id);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to extract entities for news {NewsId}, will retry in next cycle",
                    item.Id);
            }

            if (_options.DelayBetweenItemsMs > 0)
            {
                await Task.Delay(_options.DelayBetweenItemsMs, cancellationToken);
            }
        }

        if (processedCount > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Entity extraction batch saved: {ProcessedCount} news items",
                processedCount);
        }

        return processedCount;
    }

    private async Task PersistMentionsAsync(
        NewsItem item,
        IReadOnlyList<ExtractedEntity> extracted,
        DateTimeOffset extractedAt,
        Dictionary<string, NamedEntity> entityCache,
        CancellationToken cancellationToken)
    {
        if (extracted.Count == 0)
        {
            return;
        }

        var normalizedKeys = extracted
            .Select(entity => EntityNormalization.CreateNormalizedKey(entity.Name, entity.Type))
            .Distinct()
            .ToList();

        var missingKeys = normalizedKeys
            .Where(key => !entityCache.ContainsKey(key))
            .ToList();

        if (missingKeys.Count > 0)
        {
            var entitiesFromDb = await _db.NamedEntities
                .Where(entity => missingKeys.Contains(entity.NormalizedKey))
                .ToListAsync(cancellationToken);

            foreach (var entity in entitiesFromDb)
            {
                entityCache[entity.NormalizedKey] = entity;
            }
        }

        foreach (var entity in extracted)
        {
            var normalizedKey = EntityNormalization.CreateNormalizedKey(entity.Name, entity.Type);
            if (!entityCache.TryGetValue(normalizedKey, out var namedEntity))
            {
                namedEntity = new NamedEntity
                {
                    Id = Guid.NewGuid(),
                    Name = entity.Name,
                    Type = entity.Type,
                    NormalizedKey = normalizedKey,
                    CreatedAt = extractedAt,
                    UpdatedAt = extractedAt
                };

                _db.NamedEntities.Add(namedEntity);
                entityCache[normalizedKey] = namedEntity;
            }
            else if (!string.Equals(namedEntity.Name, entity.Name, StringComparison.Ordinal))
            {
                namedEntity.Name = entity.Name;
                namedEntity.UpdatedAt = extractedAt;
            }

            _db.NewsEntityMentions.Add(new NewsEntityMention
            {
                Id = Guid.NewGuid(),
                NewsItemId = item.Id,
                NamedEntityId = namedEntity.Id,
                MentionText = entity.Name,
                CreatedAt = extractedAt
            });
        }
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
            var excerpt = body.Length > 3_000 ? body[..3_000] : body;
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
        _logger.LogDebug("Ollama entity extraction for news {NewsId}: {Response}", newsId, content);
        return content;
    }
}

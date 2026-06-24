using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewsCollector.Application.Abstractions;
using NewsCollector.Domain.Entities;
using NewsCollector.Infrastructure.Linking;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class NewsEmbeddingStore : INewsEmbeddingStore
{
    private readonly NewsCollectorDbContext _db;
    private readonly IOllamaEmbeddingService _embeddingService;
    private readonly ILogger<NewsEmbeddingStore> _logger;

    public NewsEmbeddingStore(
        NewsCollectorDbContext db,
        IOllamaEmbeddingService embeddingService,
        ILogger<NewsEmbeddingStore> logger)
    {
        _db = db;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task<IReadOnlyDictionary<Guid, float[]>> EnsureEmbeddingsAsync(
        IReadOnlyList<NewsEmbeddingCandidate> candidates,
        string model,
        int maxNewEmbeddings,
        CancellationToken cancellationToken = default)
    {
        if (candidates.Count == 0)
        {
            return new Dictionary<Guid, float[]>();
        }

        var candidateIds = candidates.Select(c => c.Id).ToList();
        var existing = await _db.NewsItemEmbeddings
            .AsNoTracking()
            .Where(item => candidateIds.Contains(item.NewsItemId) && item.Model == model)
            .ToDictionaryAsync(item => item.NewsItemId, item => item.Vector, cancellationToken);

        var result = new Dictionary<Guid, float[]>(existing);
        if (maxNewEmbeddings <= 0)
        {
            return result;
        }

        var created = 0;
        foreach (var candidate in candidates)
        {
            if (result.ContainsKey(candidate.Id))
            {
                continue;
            }

            if (created >= maxNewEmbeddings)
            {
                break;
            }

            var text = TitleSimilarityCalculator.CombineTextForEmbedding(candidate.Title, candidate.Summary);
            var vector = await _embeddingService.EmbedTextAsync(text, model, cancellationToken);
            if (vector is null)
            {
                continue;
            }

            var entity = new NewsItemEmbedding
            {
                NewsItemId = candidate.Id,
                Model = model,
                Vector = vector,
                CreatedAt = DateTimeOffset.UtcNow
            };

            try
            {
                _db.NewsItemEmbeddings.Add(entity);
                await _db.SaveChangesAsync(cancellationToken);
                _db.Entry(entity).State = EntityState.Detached;
                result[candidate.Id] = vector;
                created++;
            }
            catch (DbUpdateException ex)
            {
                _db.Entry(entity).State = EntityState.Detached;
                _logger.LogDebug(ex, "Embedding for news {NewsId} already exists", candidate.Id);

                var stored = await _db.NewsItemEmbeddings
                    .AsNoTracking()
                    .Where(item => item.NewsItemId == candidate.Id && item.Model == model)
                    .Select(item => item.Vector)
                    .FirstOrDefaultAsync(cancellationToken);

                if (stored is { Length: > 0 })
                {
                    result[candidate.Id] = stored;
                }
            }
        }

        if (created > 0)
        {
            _logger.LogInformation("Stored {Count} new embeddings (model {Model})", created, model);
        }

        return result;
    }
}

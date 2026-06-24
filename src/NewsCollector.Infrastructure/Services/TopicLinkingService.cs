using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Options;
using NewsCollector.Domain.Entities;
using NewsCollector.Domain.Enums;
using NewsCollector.Infrastructure.Linking;
using NewsCollector.Infrastructure.Persistence;
using Npgsql;

namespace NewsCollector.Infrastructure.Services;

public sealed class TopicLinkingService : ITopicLinkingService
{
    private readonly NewsCollectorDbContext _db;
    private readonly TopicLinkerOptions _options;
    private readonly INewsEmbeddingStore _embeddingStore;
    private readonly ILogger<TopicLinkingService> _logger;

    public TopicLinkingService(
        NewsCollectorDbContext db,
        IOptions<TopicLinkerOptions> options,
        INewsEmbeddingStore embeddingStore,
        ILogger<TopicLinkingService> logger)
    {
        _db = db;
        _options = options.Value;
        _embeddingStore = embeddingStore;
        _logger = logger;
    }

    public async Task<int> LinkSameTopicNewsAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddHours(-_options.LookbackHours);

        var candidates = await _db.NewsItems
            .AsNoTracking()
            .Where(n => n.PublishedAt == null || n.PublishedAt >= cutoff)
            .OrderByDescending(n => n.PublishedAt ?? n.FetchedAt)
            .Take(_options.MaxCandidates)
            .Select(n => new NewsCandidate(n.Id, n.Title, n.Summary))
            .ToListAsync(cancellationToken);

        if (candidates.Count < 2)
        {
            return 0;
        }

        var candidateIds = candidates.Select(c => c.Id).ToHashSet();
        var existingPairs = await LoadExistingPairsAsync(candidateIds, cancellationToken);

        IReadOnlyDictionary<Guid, float[]> embeddings = new Dictionary<Guid, float[]>();
        if (_options.UseEmbeddings)
        {
            embeddings = await _embeddingStore.EnsureEmbeddingsAsync(
                candidates.Select(c => new NewsEmbeddingCandidate(c.Id, c.Title, c.Summary)).ToList(),
                _options.EmbeddingModel,
                _options.MaxEmbeddingsPerCycle,
                cancellationToken);
        }

        IReadOnlyDictionary<Guid, HashSet<Guid>> entitySets = new Dictionary<Guid, HashSet<Guid>>();
        if (_options.UseEntityOverlap)
        {
            entitySets = await LoadEntitySetsAsync(candidateIds, cancellationToken);
        }

        var linksToCreate = new List<NewsLink>();

        for (var i = 0; i < candidates.Count; i++)
        {
            for (var j = i + 1; j < candidates.Count; j++)
            {
                var left = candidates[i];
                var right = candidates[j];
                var (lowId, highId) = OrderIds(left.Id, right.Id);

                if (existingPairs.Contains((lowId, highId)))
                {
                    continue;
                }

                var titleJaccard = TitleSimilarityCalculator.ComputeSimilarity(
                    left.Title,
                    left.Summary,
                    right.Title,
                    right.Summary);

                var sharedTokens = TitleSimilarityCalculator.CountSharedTokens(
                    left.Title,
                    left.Summary,
                    right.Title,
                    right.Summary);

                var embeddingCosine = 0m;
                if (_options.UseEmbeddings
                    && embeddings.TryGetValue(left.Id, out var leftVector)
                    && embeddings.TryGetValue(right.Id, out var rightVector))
                {
                    embeddingCosine = EmbeddingSimilarityCalculator.CosineSimilarity(leftVector, rightVector);
                }

                var leftEntities = entitySets.GetValueOrDefault(left.Id) ?? [];
                var rightEntities = entitySets.GetValueOrDefault(right.Id) ?? [];
                var entityOverlap = EntityOverlapCalculator.Compute(leftEntities, rightEntities);

                if (ShouldSkipPair(titleJaccard, sharedTokens, embeddingCosine, entityOverlap))
                {
                    continue;
                }

                var signals = new TopicLinkSignals(
                    titleJaccard,
                    sharedTokens,
                    embeddingCosine,
                    entityOverlap.SharedCount,
                    entityOverlap.Jaccard);

                var decision = TopicLinkClassifier.Classify(signals, _options);
                if (decision is null)
                {
                    continue;
                }

                linksToCreate.Add(new NewsLink
                {
                    Id = Guid.NewGuid(),
                    NewsIdLow = lowId,
                    NewsIdHigh = highId,
                    LinkType = decision.Value.LinkType,
                    LinkMethod = decision.Value.LinkMethod,
                    Confidence = decision.Value.Confidence,
                    CreatedAt = DateTimeOffset.UtcNow
                });

                existingPairs.Add((lowId, highId));
            }
        }

        if (linksToCreate.Count == 0)
        {
            return 0;
        }

        return await PersistLinksAsync(linksToCreate, cancellationToken);
    }

    private bool ShouldSkipPair(
        decimal titleJaccard,
        int sharedTokens,
        decimal embeddingCosine,
        EntityOverlapResult entityOverlap)
    {
        if (titleJaccard >= _options.MinRelatedTitleSimilarity && sharedTokens >= 1)
        {
            return false;
        }

        if (embeddingCosine >= _options.MinEmbeddingSimilarityForRelated)
        {
            return false;
        }

        if (entityOverlap.SharedCount >= _options.MinSharedEntitiesForRelated)
        {
            return false;
        }

        return titleJaccard < _options.MinRelatedTitleSimilarity;
    }

    private async Task<IReadOnlyDictionary<Guid, HashSet<Guid>>> LoadEntitySetsAsync(
        HashSet<Guid> candidateIds,
        CancellationToken cancellationToken)
    {
        var ids = candidateIds.ToList();

        var rows = await _db.NewsEntityMentions
            .AsNoTracking()
            .Where(mention => ids.Contains(mention.NewsItemId))
            .Select(mention => new { mention.NewsItemId, mention.NamedEntityId })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(row => row.NewsItemId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(row => row.NamedEntityId).ToHashSet());
    }

    private async Task<HashSet<(Guid Low, Guid High)>> LoadExistingPairsAsync(
        HashSet<Guid> candidateIds,
        CancellationToken cancellationToken)
    {
        var ids = candidateIds.ToList();

        var pairs = await _db.NewsLinks
            .AsNoTracking()
            .Where(l => ids.Contains(l.NewsIdLow) || ids.Contains(l.NewsIdHigh))
            .Select(l => new { l.NewsIdLow, l.NewsIdHigh })
            .ToListAsync(cancellationToken);

        return pairs
            .Select(p => (p.NewsIdLow, p.NewsIdHigh))
            .ToHashSet();
    }

    private async Task<int> PersistLinksAsync(
        IReadOnlyList<NewsLink> links,
        CancellationToken cancellationToken)
    {
        const int batchSize = 100;
        var inserted = 0;

        foreach (var batch in links.Chunk(batchSize))
        {
            try
            {
                _db.NewsLinks.AddRange(batch);
                await _db.SaveChangesAsync(cancellationToken);
                inserted += batch.Length;

                foreach (var link in batch)
                {
                    _db.Entry(link).State = EntityState.Detached;
                }
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                foreach (var link in batch)
                {
                    _db.Entry(link).State = EntityState.Detached;
                }

                _logger.LogDebug("Some topic links already exist, inserting individually");

                foreach (var link in batch)
                {
                    _db.Entry(link).State = EntityState.Detached;

                    try
                    {
                        _db.NewsLinks.Add(link);
                        await _db.SaveChangesAsync(cancellationToken);
                        inserted++;
                    }
                    catch (DbUpdateException singleEx) when (IsUniqueViolation(singleEx))
                    {
                        // Race with another linker cycle or duplicate pair.
                    }
                    finally
                    {
                        _db.Entry(link).State = EntityState.Detached;
                    }
                }
            }
        }

        _logger.LogInformation("Created {Count} topic links", inserted);
        return inserted;
    }

    private static (Guid Low, Guid High) OrderIds(Guid first, Guid second) =>
        first.CompareTo(second) < 0 ? (first, second) : (second, first);

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    private sealed record NewsCandidate(Guid Id, string Title, string? Summary);
}

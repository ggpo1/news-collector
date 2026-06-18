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
    private readonly ILogger<TopicLinkingService> _logger;

    public TopicLinkingService(
        NewsCollectorDbContext db,
        IOptions<TopicLinkerOptions> options,
        ILogger<TopicLinkingService> logger)
    {
        _db = db;
        _options = options.Value;
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

                var similarity = TitleSimilarityCalculator.ComputeSimilarity(
                    left.Title,
                    left.Summary,
                    right.Title,
                    right.Summary);

                if (similarity < _options.MinSimilarity)
                {
                    continue;
                }

                var sharedTokens = TitleSimilarityCalculator.CountSharedTokens(
                    left.Title,
                    left.Summary,
                    right.Title,
                    right.Summary);

                if (sharedTokens < _options.MinSharedTokens)
                {
                    continue;
                }

                var linkType = similarity >= _options.DuplicateSimilarity
                    ? LinkType.Duplicate
                    : LinkType.SameTopic;

                linksToCreate.Add(new NewsLink
                {
                    Id = Guid.NewGuid(),
                    NewsIdLow = lowId,
                    NewsIdHigh = highId,
                    LinkType = linkType,
                    LinkMethod = LinkMethod.TitleSimilarity,
                    Confidence = similarity,
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

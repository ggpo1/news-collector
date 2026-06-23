using Microsoft.EntityFrameworkCore;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Enums;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

public sealed class NewsEntityGraphQueryService : INewsEntityGraphQueryService
{
    private readonly NewsCollectorDbContext _db;

    public NewsEntityGraphQueryService(NewsCollectorDbContext db)
    {
        _db = db;
    }

    public async Task<EntityGraphDto> GetCoMentionGraphAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        NamedEntityType? entityType = null,
        int minWeight = 2,
        int maxNodes = 150,
        CancellationToken cancellationToken = default)
    {
        var periodTo = to ?? DateTimeOffset.UtcNow;
        var periodFrom = from ?? periodTo.AddDays(-7);
        if (periodFrom > periodTo)
        {
            (periodFrom, periodTo) = (periodTo, periodFrom);
        }

        maxNodes = Math.Clamp(maxNodes, 10, 120);
        minWeight = Math.Max(minWeight, 1);

        var mentions = await LoadMentionsInPeriodAsync(periodFrom, periodTo, entityType, cancellationToken);
        var mentionCounts = mentions
            .GroupBy(m => m.NamedEntityId)
            .ToDictionary(g => g.Key, g => g.Count());

        var edgeWeights = BuildCoMentionWeights(mentions);
        var maxEdges = Math.Min(maxNodes * 2, 100);
        var filteredEdges = edgeWeights
            .Where(pair => pair.Value >= minWeight)
            .OrderByDescending(pair => pair.Value)
            .Take(maxEdges)
            .ToList();

        var nodeIds = filteredEdges
            .SelectMany(edge => new[] { edge.Key.SourceId, edge.Key.TargetId })
            .Distinct()
            .OrderByDescending(id => mentionCounts.GetValueOrDefault(id))
            .Take(maxNodes)
            .ToHashSet();

        filteredEdges = filteredEdges
            .Where(edge => nodeIds.Contains(edge.Key.SourceId) && nodeIds.Contains(edge.Key.TargetId))
            .ToList();

        var entities = await _db.NamedEntities
            .AsNoTracking()
            .Where(entity => nodeIds.Contains(entity.Id))
            .ToDictionaryAsync(entity => entity.Id, cancellationToken);

        var nodes = nodeIds
            .Select(id => entities[id])
            .OrderByDescending(entity => mentionCounts.GetValueOrDefault(entity.Id))
            .Select(entity => new EntityGraphNodeDto(
                entity.Id,
                entity.Name,
                entity.Type,
                mentionCounts.GetValueOrDefault(entity.Id)))
            .ToList();

        var edges = filteredEdges
            .Select(edge => new EntityGraphEdgeDto(edge.Key.SourceId, edge.Key.TargetId, edge.Value))
            .OrderByDescending(edge => edge.Weight)
            .ToList();

        return new EntityGraphDto(periodFrom, periodTo, nodes, edges);
    }

    public async Task<IReadOnlyList<NamedEntityListDto>> SearchEntitiesAsync(
        string? query,
        NamedEntityType? entityType = null,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 200);

        var entitiesQuery = _db.NamedEntities.AsNoTracking();

        if (entityType.HasValue)
        {
            entitiesQuery = entitiesQuery.Where(entity => entity.Type == entityType.Value);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            entitiesQuery = entitiesQuery.Where(entity => EF.Functions.ILike(entity.Name, pattern));
        }

        return await entitiesQuery
            .Select(entity => new NamedEntityListDto(
                entity.Id,
                entity.Name,
                entity.Type,
                entity.Mentions.Count))
            .OrderByDescending(entity => entity.MentionCount)
            .ThenBy(entity => entity.Name)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<NamedEntityDetailDto?> GetEntityDetailAsync(
        Guid entityId,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
    {
        var entity = await _db.NamedEntities
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == entityId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var periodTo = to ?? DateTimeOffset.UtcNow;
        var periodFrom = from ?? periodTo.AddDays(-30);

        var mentions = await LoadMentionsInPeriodAsync(periodFrom, periodTo, null, cancellationToken);
        var mentionCounts = mentions
            .GroupBy(m => m.NamedEntityId)
            .ToDictionary(g => g.Key, g => g.Count());

        if (!mentionCounts.ContainsKey(entityId))
        {
            return new NamedEntityDetailDto(
                entity.Id,
                entity.Name,
                entity.Type,
                0,
                []);
        }

        var edgeWeights = BuildCoMentionWeights(mentions);
        var neighbors = edgeWeights
            .Where(edge => edge.Key.SourceId == entityId || edge.Key.TargetId == entityId)
            .Select(edge =>
            {
                var neighborId = edge.Key.SourceId == entityId ? edge.Key.TargetId : edge.Key.SourceId;
                return new { NeighborId = neighborId, edge.Value };
            })
            .OrderByDescending(edge => edge.Value)
            .Take(25)
            .ToList();

        var neighborIds = neighbors.Select(neighbor => neighbor.NeighborId).ToList();
        var neighborEntities = await _db.NamedEntities
            .AsNoTracking()
            .Where(item => neighborIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, cancellationToken);

        var neighborDtos = neighbors
            .Where(neighbor => neighborEntities.ContainsKey(neighbor.NeighborId))
            .Select(neighbor =>
            {
                var neighborEntity = neighborEntities[neighbor.NeighborId];
                return new NamedEntityNeighborDto(
                    neighborEntity.Id,
                    neighborEntity.Name,
                    neighborEntity.Type,
                    neighbor.Value);
            })
            .ToList();

        return new NamedEntityDetailDto(
            entity.Id,
            entity.Name,
            entity.Type,
            mentionCounts.GetValueOrDefault(entityId),
            neighborDtos);
    }

    private async Task<List<MentionRow>> LoadMentionsInPeriodAsync(
        DateTimeOffset periodFrom,
        DateTimeOffset periodTo,
        NamedEntityType? entityType,
        CancellationToken cancellationToken)
    {
        var query =
            from mention in _db.NewsEntityMentions.AsNoTracking()
            join news in _db.NewsItems.AsNoTracking() on mention.NewsItemId equals news.Id
            join entity in _db.NamedEntities.AsNoTracking() on mention.NamedEntityId equals entity.Id
            where (news.PublishedAt ?? news.FetchedAt) >= periodFrom
                  && (news.PublishedAt ?? news.FetchedAt) <= periodTo
            select new MentionRow(mention.NewsItemId, mention.NamedEntityId, entity.Type);

        if (entityType.HasValue)
        {
            query = query.Where(row => row.Type == entityType.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    private static Dictionary<(Guid SourceId, Guid TargetId), int> BuildCoMentionWeights(
        IReadOnlyList<MentionRow> mentions)
    {
        var edgeWeights = new Dictionary<(Guid, Guid), int>();

        foreach (var group in mentions.GroupBy(mention => mention.NewsItemId))
        {
            var entityIds = group.Select(mention => mention.NamedEntityId).Distinct().OrderBy(id => id).ToList();
            for (var i = 0; i < entityIds.Count; i++)
            {
                for (var j = i + 1; j < entityIds.Count; j++)
                {
                    var key = (entityIds[i], entityIds[j]);
                    edgeWeights[key] = edgeWeights.GetValueOrDefault(key) + 1;
                }
            }
        }

        return edgeWeights;
    }

    private sealed record MentionRow(Guid NewsItemId, Guid NamedEntityId, NamedEntityType Type);
}

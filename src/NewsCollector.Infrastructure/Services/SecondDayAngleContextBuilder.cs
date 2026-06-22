using Microsoft.EntityFrameworkCore;
using NewsCollector.Domain.Entities;
using NewsCollector.Domain.Enums;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

internal sealed class SecondDayAngleContextBuilder
{
    private const int HistoricalMinAgeDays = 30;
    private const int MaxHistoricalCandidates = 12;
    private const int MaxSameTopicCoverage = 8;
    private const int MaxCoMentionEntities = 15;

    private readonly NewsCollectorDbContext _db;

    public SecondDayAngleContextBuilder(NewsCollectorDbContext db)
    {
        _db = db;
    }

    public async Task<SecondDayAngleContext?> BuildAsync(Guid newsId, CancellationToken cancellationToken)
    {
        var source = await _db.NewsItems
            .AsNoTracking()
            .Include(n => n.Source)
            .Include(n => n.Category)
            .FirstOrDefaultAsync(n => n.Id == newsId, cancellationToken);

        if (source is null)
        {
            return null;
        }

        var sourceDate = source.PublishedAt ?? source.FetchedAt;
        var historicalBefore = sourceDate.AddDays(-HistoricalMinAgeDays);
        var sameTopicIds = await LoadSameTopicNewsIdsAsync(newsId, cancellationToken);

        var sourceEntities = await LoadSourceEntitiesAsync(newsId, cancellationToken);
        var historicalCandidates = await LoadHistoricalCandidatesAsync(
            newsId,
            source,
            sourceDate,
            historicalBefore,
            sameTopicIds,
            sourceEntities,
            cancellationToken);

        var sameTopicCoverage = await LoadSameTopicCoverageAsync(
            newsId,
            source.SourceId,
            sameTopicIds,
            cancellationToken);

        var coMentionEntities = await LoadCoMentionEntitiesAsync(
            newsId,
            sourceEntities,
            cancellationToken);

        return new SecondDayAngleContext(
            source,
            sourceEntities,
            historicalCandidates,
            sameTopicCoverage,
            coMentionEntities);
    }

    private async Task<IReadOnlyList<SourceEntityContext>> LoadSourceEntitiesAsync(
        Guid newsId,
        CancellationToken cancellationToken) =>
        await _db.NewsEntityMentions
            .AsNoTracking()
            .Where(m => m.NewsItemId == newsId)
            .Select(m => new SourceEntityContext(
                m.NamedEntity.Name,
                m.NamedEntity.Type,
                m.MentionText))
            .ToListAsync(cancellationToken);

    private async Task<HashSet<Guid>> LoadSameTopicNewsIdsAsync(
        Guid newsId,
        CancellationToken cancellationToken)
    {
        var linkedIds = await _db.NewsLinks
            .AsNoTracking()
            .Where(l => l.LinkType == LinkType.SameTopic
                        && (l.NewsIdLow == newsId || l.NewsIdHigh == newsId))
            .Select(l => l.NewsIdLow == newsId ? l.NewsIdHigh : l.NewsIdLow)
            .ToListAsync(cancellationToken);

        return linkedIds.ToHashSet();
    }

    private async Task<IReadOnlyList<HistoricalCandidateContext>> LoadHistoricalCandidatesAsync(
        Guid newsId,
        NewsItem source,
        DateTimeOffset sourceDate,
        DateTimeOffset historicalBefore,
        HashSet<Guid> sameTopicIds,
        IReadOnlyList<SourceEntityContext> sourceEntities,
        CancellationToken cancellationToken)
    {
        var sourceEntityIds = await _db.NewsEntityMentions
            .AsNoTracking()
            .Where(m => m.NewsItemId == newsId)
            .Select(m => m.NamedEntityId)
            .ToListAsync(cancellationToken);

        var candidates = new Dictionary<Guid, HistoricalCandidateContext>();

        if (sourceEntityIds.Count > 0)
        {
            var entityMatches = await (
                from mention in _db.NewsEntityMentions.AsNoTracking()
                join news in _db.NewsItems.AsNoTracking() on mention.NewsItemId equals news.Id
                join src in _db.Sources.AsNoTracking() on news.SourceId equals src.Id
                where sourceEntityIds.Contains(mention.NamedEntityId)
                      && news.Id != newsId
                      && !sameTopicIds.Contains(news.Id)
                      && (news.PublishedAt ?? news.FetchedAt) < historicalBefore
                group mention by new
                {
                    news.Id,
                    news.Title,
                    news.Summary,
                    news.Content,
                    news.PublishedAt,
                    news.FetchedAt,
                    SourceName = src.Name
                }
                into grouped
                orderby grouped.Count() descending
                select new HistoricalCandidateContext(
                    grouped.Key.Id,
                    grouped.Key.Title,
                    grouped.Key.Summary,
                    grouped.Key.Content,
                    grouped.Key.PublishedAt ?? grouped.Key.FetchedAt,
                    grouped.Key.SourceName,
                    grouped.Count()))
                .Take(MaxHistoricalCandidates)
                .ToListAsync(cancellationToken);

            foreach (var candidate in entityMatches)
            {
                candidates[candidate.NewsId] = candidate;
            }
        }

        if (candidates.Count < 5 && source.CategoryId.HasValue)
        {
            var categoryMatches = await _db.NewsItems
                .AsNoTracking()
                .Where(n => n.Id != newsId
                            && n.CategoryId == source.CategoryId
                            && !sameTopicIds.Contains(n.Id)
                            && (n.PublishedAt ?? n.FetchedAt) < historicalBefore)
                .OrderByDescending(n => n.PublishedAt ?? n.FetchedAt)
                .Take(MaxHistoricalCandidates)
                .Select(n => new HistoricalCandidateContext(
                    n.Id,
                    n.Title,
                    n.Summary,
                    n.Content,
                    n.PublishedAt ?? n.FetchedAt,
                    n.Source.Name,
                    0))
                .ToListAsync(cancellationToken);

            foreach (var candidate in categoryMatches)
            {
                candidates.TryAdd(candidate.NewsId, candidate);
            }
        }

        return candidates.Values
            .OrderByDescending(c => c.SharedEntityCount)
            .ThenByDescending(c => c.PublishedAt)
            .Take(MaxHistoricalCandidates)
            .ToList();
    }

    private async Task<IReadOnlyList<SameTopicCoverageContext>> LoadSameTopicCoverageAsync(
        Guid newsId,
        Guid sourceSourceId,
        HashSet<Guid> sameTopicIds,
        CancellationToken cancellationToken)
    {
        if (sameTopicIds.Count == 0)
        {
            return [];
        }

        return await _db.NewsItems
            .AsNoTracking()
            .Where(n => sameTopicIds.Contains(n.Id) && n.SourceId != sourceSourceId)
            .OrderByDescending(n => n.PublishedAt ?? n.FetchedAt)
            .Take(MaxSameTopicCoverage)
            .Select(n => new SameTopicCoverageContext(
                n.Id,
                n.Title,
                n.Summary,
                n.Content,
                n.Source.Name))
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<CoMentionEntityContext>> LoadCoMentionEntitiesAsync(
        Guid newsId,
        IReadOnlyList<SourceEntityContext> sourceEntities,
        CancellationToken cancellationToken)
    {
        if (sourceEntities.Count == 0)
        {
            return [];
        }

        var sourceEntityIds = await _db.NewsEntityMentions
            .AsNoTracking()
            .Where(m => m.NewsItemId == newsId)
            .Select(m => m.NamedEntityId)
            .ToListAsync(cancellationToken);

        if (sourceEntityIds.Count == 0)
        {
            return [];
        }

        var coMentions = await (
            from sourceMention in _db.NewsEntityMentions.AsNoTracking()
            from otherMention in _db.NewsEntityMentions.AsNoTracking()
            where sourceMention.NewsItemId == newsId
                  && sourceMention.NewsItemId == otherMention.NewsItemId
                  && sourceMention.NamedEntityId != otherMention.NamedEntityId
            group otherMention by new
            {
                otherMention.NamedEntityId,
                otherMention.NamedEntity.Name,
                otherMention.NamedEntity.Type
            }
            into grouped
            orderby grouped.Count() descending
            select new CoMentionEntityContext(
                grouped.Key.Name,
                grouped.Key.Type,
                grouped.Count()))
            .Take(MaxCoMentionEntities)
            .ToListAsync(cancellationToken);

        if (coMentions.Count > 0)
        {
            return coMentions;
        }

        return await (
            from mention in _db.NewsEntityMentions.AsNoTracking()
            join entity in _db.NamedEntities.AsNoTracking() on mention.NamedEntityId equals entity.Id
            where sourceEntityIds.Contains(mention.NamedEntityId)
                  && mention.NewsItemId != newsId
            group mention by new { entity.Name, entity.Type }
            into grouped
            orderby grouped.Count() descending
            select new CoMentionEntityContext(
                grouped.Key.Name,
                grouped.Key.Type,
                grouped.Count()))
            .Take(MaxCoMentionEntities)
            .ToListAsync(cancellationToken);
    }
}

internal sealed record SecondDayAngleContext(
    NewsItem Source,
    IReadOnlyList<SourceEntityContext> SourceEntities,
    IReadOnlyList<HistoricalCandidateContext> HistoricalCandidates,
    IReadOnlyList<SameTopicCoverageContext> SameTopicCoverage,
    IReadOnlyList<CoMentionEntityContext> CoMentionEntities);

internal sealed record SourceEntityContext(string Name, NamedEntityType Type, string MentionText);

internal sealed record HistoricalCandidateContext(
    Guid NewsId,
    string Title,
    string? Summary,
    string? Content,
    DateTimeOffset PublishedAt,
    string SourceName,
    int SharedEntityCount);

internal sealed record SameTopicCoverageContext(
    Guid NewsId,
    string Title,
    string? Summary,
    string? Content,
    string SourceName);

internal sealed record CoMentionEntityContext(string Name, NamedEntityType Type, int CoMentionCount);

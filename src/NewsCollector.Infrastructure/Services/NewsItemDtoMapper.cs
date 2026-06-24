using Microsoft.EntityFrameworkCore;
using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Entities;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Services;

internal static class NewsItemDtoMapper
{
    public static EditorialTagDto MapTag(EditorialTag tag) =>
        new(tag.Id, tag.Slug, tag.Name, tag.Color);

    public static EditorialTagDto MapTag(NewsEditorialTag link) =>
        MapTag(link.EditorialTag);

    public static async Task<IReadOnlyDictionary<Guid, IReadOnlyList<EditorialTagDto>>> LoadTagsByNewsIdsAsync(
        NewsCollectorDbContext db,
        IReadOnlyCollection<Guid> newsIds,
        CancellationToken cancellationToken)
    {
        if (newsIds.Count == 0)
        {
            return new Dictionary<Guid, IReadOnlyList<EditorialTagDto>>();
        }

        var rows = await db.NewsEditorialTags
            .AsNoTracking()
            .Include(item => item.EditorialTag)
            .Where(item => newsIds.Contains(item.NewsItemId))
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(item => item.NewsItemId)
            .ToDictionary(
                group => group.Key,
                IReadOnlyList<EditorialTagDto> (group) => group
                    .Select(MapTag)
                    .OrderBy(tag => tag.Name)
                    .ToList());
    }
}

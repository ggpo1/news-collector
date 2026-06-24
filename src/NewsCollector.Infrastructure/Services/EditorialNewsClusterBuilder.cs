using NewsCollector.Domain.Entities;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Infrastructure.Services;

internal static class EditorialNewsClusterBuilder
{
    public static IReadOnlyList<IReadOnlySet<Guid>> BuildClusters(
        IReadOnlyCollection<Guid> newsIds,
        IReadOnlyList<NewsLink> links)
    {
        if (newsIds.Count == 0)
        {
            return [];
        }

        var parent = newsIds.ToDictionary(id => id, id => id);

        Guid Find(Guid id)
        {
            while (!parent[id].Equals(id))
            {
                parent[id] = parent[parent[id]];
                id = parent[id];
            }

            return id;
        }

        void Union(Guid left, Guid right)
        {
            var rootLeft = Find(left);
            var rootRight = Find(right);
            if (rootLeft.Equals(rootRight))
            {
                return;
            }

            if (string.CompareOrdinal(rootLeft.ToString(), rootRight.ToString()) < 0)
            {
                parent[rootRight] = rootLeft;
            }
            else
            {
                parent[rootLeft] = rootRight;
            }
        }

        foreach (var link in links)
        {
            if (newsIds.Contains(link.NewsIdLow) && newsIds.Contains(link.NewsIdHigh))
            {
                Union(link.NewsIdLow, link.NewsIdHigh);
            }
        }

        var groups = new Dictionary<Guid, HashSet<Guid>>();
        foreach (var id in newsIds)
        {
            var root = Find(id);
            if (!groups.TryGetValue(root, out var bucket))
            {
                bucket = [];
                groups[root] = bucket;
            }

            bucket.Add(id);
        }

        return groups.Values
            .Where(group => group.Count > 0)
            .Select<IReadOnlySet<Guid>, IReadOnlySet<Guid>>(group => group)
            .ToList();
    }

    public static bool ClusterHasDuplicateLink(
        IReadOnlySet<Guid> clusterIds,
        IReadOnlyList<NewsLink> links) =>
        links.Any(link =>
            link.LinkType == LinkType.Duplicate
            && clusterIds.Contains(link.NewsIdLow)
            && clusterIds.Contains(link.NewsIdHigh));
}

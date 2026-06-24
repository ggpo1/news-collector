namespace NewsCollector.Infrastructure.Linking;

public static class EntityOverlapCalculator
{
    public static EntityOverlapResult Compute(IReadOnlySet<Guid> left, IReadOnlySet<Guid> right)
    {
        if (left.Count == 0 || right.Count == 0)
        {
            return EntityOverlapResult.Empty;
        }

        var shared = 0;
        foreach (var entityId in left)
        {
            if (right.Contains(entityId))
            {
                shared++;
            }
        }

        if (shared == 0)
        {
            return EntityOverlapResult.Empty;
        }

        var union = left.Count + right.Count - shared;
        var jaccard = union == 0 ? 0m : Math.Round((decimal)shared / union, 4);

        return new EntityOverlapResult(shared, jaccard);
    }
}

public readonly record struct EntityOverlapResult(int SharedCount, decimal Jaccard)
{
    public static EntityOverlapResult Empty { get; } = new(0, 0m);
}

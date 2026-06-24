namespace NewsCollector.Application.Abstractions;

public interface INewsEmbeddingStore
{
    Task<IReadOnlyDictionary<Guid, float[]>> EnsureEmbeddingsAsync(
        IReadOnlyList<NewsEmbeddingCandidate> candidates,
        string model,
        int maxNewEmbeddings,
        CancellationToken cancellationToken = default);
}

public sealed record NewsEmbeddingCandidate(Guid Id, string Title, string? Summary);

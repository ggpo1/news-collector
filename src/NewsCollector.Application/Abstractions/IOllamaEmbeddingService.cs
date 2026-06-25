namespace NewsCollector.Application.Abstractions;

public interface IOllamaEmbeddingService
{
    Task<float[]?> EmbedTextAsync(string text, string model, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<float[]?>> EmbedTextsAsync(
        IReadOnlyList<string> texts,
        string model,
        CancellationToken cancellationToken = default);
}

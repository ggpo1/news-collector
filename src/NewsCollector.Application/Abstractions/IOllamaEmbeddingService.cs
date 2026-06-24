namespace NewsCollector.Application.Abstractions;

public interface IOllamaEmbeddingService
{
    Task<float[]?> EmbedTextAsync(string text, string model, CancellationToken cancellationToken = default);
}

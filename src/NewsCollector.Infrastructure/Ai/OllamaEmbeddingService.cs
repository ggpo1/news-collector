using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using NewsCollector.Application.Abstractions;

namespace NewsCollector.Infrastructure.Ai;

public sealed class OllamaEmbeddingService : IOllamaEmbeddingService
{
    private const int MaxPromptLength = 512;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OllamaEmbeddingService> _logger;

    public OllamaEmbeddingService(
        IHttpClientFactory httpClientFactory,
        ILogger<OllamaEmbeddingService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<float[]?> EmbedTextAsync(
        string text,
        string model,
        CancellationToken cancellationToken = default)
    {
        var prompt = Truncate(text.Trim());
        if (prompt.Length == 0)
        {
            return null;
        }

        var client = _httpClientFactory.CreateClient("Ollama");
        var request = new OllamaEmbeddingRequest(model, prompt);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(120));

        try
        {
            var response = await client.PostAsJsonAsync("api/embeddings", request, timeoutCts.Token);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(timeoutCts.Token);
                _logger.LogWarning(
                    "Ollama embeddings returned {StatusCode}: {Body}",
                    (int)response.StatusCode,
                    body);
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(timeoutCts.Token);
            if (payload?.Embedding is not { Length: > 0 })
            {
                _logger.LogWarning("Ollama embeddings returned empty vector for model {Model}", model);
                return null;
            }

            return payload.Embedding;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to compute embedding with model {Model}", model);
            return null;
        }
    }

    private static string Truncate(string text) =>
        text.Length <= MaxPromptLength ? text : text[..MaxPromptLength];

    private sealed record OllamaEmbeddingRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("prompt")] string Prompt);

    private sealed record OllamaEmbeddingResponse(
        [property: JsonPropertyName("embedding")] float[]? Embedding);
}

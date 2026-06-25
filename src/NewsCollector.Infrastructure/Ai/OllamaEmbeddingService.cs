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
        var vectors = await EmbedTextsAsync([text], model, cancellationToken);
        return vectors.Count > 0 ? vectors[0] : null;
    }

    public async Task<IReadOnlyList<float[]?>> EmbedTextsAsync(
        IReadOnlyList<string> texts,
        string model,
        CancellationToken cancellationToken = default)
    {
        if (texts.Count == 0)
        {
            return [];
        }

        var prompts = texts
            .Select(Truncate)
            .Where(text => text.Length > 0)
            .ToList();

        if (prompts.Count == 0)
        {
            return texts.Select(_ => (float[]?)null).ToList();
        }

        var client = _httpClientFactory.CreateClient("Ollama");

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(120));

        var vectors = await TryEmbedWithCurrentApiAsync(client, prompts, model, timeoutCts.Token);
        if (vectors is null)
        {
            vectors = await TryEmbedWithLegacyApiAsync(client, prompts, model, timeoutCts.Token);
        }

        if (vectors is null)
        {
            return texts.Select(_ => (float[]?)null).ToList();
        }

        if (texts.Count == 1)
        {
            return vectors.Count > 0 ? [vectors[0]] : [null];
        }

        return vectors;
    }

    private async Task<IReadOnlyList<float[]?>?> TryEmbedWithCurrentApiAsync(
        HttpClient client,
        IReadOnlyList<string> prompts,
        string model,
        CancellationToken cancellationToken)
    {
        object input = prompts.Count == 1 ? prompts[0] : prompts;
        var request = new OllamaEmbedRequest(model, input);

        try
        {
            var response = await client.PostAsJsonAsync("api/embed", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<OllamaEmbedResponse>(cancellationToken);
            if (payload?.Embeddings is not { Count: > 0 })
            {
                return null;
            }

            return payload.Embeddings
                .Select(vector => vector is { Length: > 0 } ? vector : null)
                .ToList();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogDebug(ex, "Ollama /api/embed failed for model {Model}", model);
            return null;
        }
    }

    private async Task<IReadOnlyList<float[]?>?> TryEmbedWithLegacyApiAsync(
        HttpClient client,
        IReadOnlyList<string> prompts,
        string model,
        CancellationToken cancellationToken)
    {
        if (prompts.Count > 1)
        {
            var vectors = new List<float[]?>(prompts.Count);
            foreach (var prompt in prompts)
            {
                vectors.Add(await EmbedWithLegacyApiAsync(client, prompt, model, cancellationToken));
            }

            return vectors;
        }

        return [await EmbedWithLegacyApiAsync(client, prompts[0], model, cancellationToken)];
    }

    private async Task<float[]?> EmbedWithLegacyApiAsync(
        HttpClient client,
        string prompt,
        string model,
        CancellationToken cancellationToken)
    {
        var request = new OllamaLegacyEmbeddingRequest(model, prompt);

        try
        {
            var response = await client.PostAsJsonAsync("api/embeddings", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "Ollama embeddings returned {StatusCode}: {Body}",
                    (int)response.StatusCode,
                    body);
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<OllamaLegacyEmbeddingResponse>(cancellationToken);
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
        text.Trim().Length <= MaxPromptLength ? text.Trim() : text.Trim()[..MaxPromptLength];

    private sealed record OllamaEmbedRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("input")] object Input);

    private sealed record OllamaEmbedResponse(
        [property: JsonPropertyName("embeddings")] List<float[]>? Embeddings);

    private sealed record OllamaLegacyEmbeddingRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("prompt")] string Prompt);

    private sealed record OllamaLegacyEmbeddingResponse(
        [property: JsonPropertyName("embedding")] float[]? Embedding);
}

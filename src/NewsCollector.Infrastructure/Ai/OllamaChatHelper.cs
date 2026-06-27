using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using NewsCollector.Application.Options;

namespace NewsCollector.Infrastructure.Ai;

public static class OllamaChatHelper
{
    public static OllamaChatApiRequest CreateRequest(
        OllamaOptions options,
        OllamaChatMessage[] messages,
        bool stream = false,
        string? format = null)
    {
        return new OllamaChatApiRequest
        {
            Model = options.Model,
            Messages = messages,
            Stream = stream,
            Format = format,
            KeepAlive = options.KeepAlive,
            Think = options.Think ? null : false,
            Options = new OllamaModelRuntimeOptions
            {
                NumCtx = options.NumCtx,
            },
        };
    }

    public static async Task<string> ReadChatContentAsync(
        HttpResponseMessage response,
        OllamaOptions options,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(FormatFailure((int)response.StatusCode, body, options));
        }

        var payload = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken);
        var content = payload?.Message?.Content;
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Ollama вернула пустой ответ.");
        }

        return content;
    }

    public static string FormatFailure(int statusCode, string body, OllamaOptions options)
    {
        if (body.Contains("signal: killed", StringComparison.OrdinalIgnoreCase)
            || body.Contains("process has terminated", StringComparison.OrdinalIgnoreCase))
        {
            return
                $"Ollama: процесс модели убит (нехватка RAM/VRAM, OOM). " +
                $"Модель «{options.Model}» для 27B нужно ~16+ GB видеопамяти/оперативки. " +
                $"Варианты: модель полегче (qwen2.5:14b, llama3.1:8b), больше памяти, " +
                $"OLLAMA_MAX_LOADED_MODELS=1, OLLAMA_NUM_CTX={options.NumCtx}. " +
                $"Ответ Ollama: {body}";
        }

        if (statusCode == (int)HttpStatusCode.InternalServerError)
        {
            return
                $"Ollama вернула 500. Модель: «{options.Model}». " +
                $"Проверьте `docker compose logs ollama` и память GPU/RAM. Ответ: {body}";
        }

        return $"Ollama вернула ошибку {statusCode}. Модель: «{options.Model}». Ответ: {body}";
    }

    public sealed class OllamaChatApiRequest
    {
        [JsonPropertyName("model")]
        public required string Model { get; init; }

        [JsonPropertyName("messages")]
        public required OllamaChatMessage[] Messages { get; init; }

        [JsonPropertyName("stream")]
        public bool Stream { get; init; }

        [JsonPropertyName("format")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Format { get; init; }

        [JsonPropertyName("keep_alive")]
        public required string KeepAlive { get; init; }

        [JsonPropertyName("think")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Think { get; init; }

        [JsonPropertyName("options")]
        public OllamaModelRuntimeOptions? Options { get; init; }
    }

    public sealed class OllamaModelRuntimeOptions
    {
        [JsonPropertyName("num_ctx")]
        public int NumCtx { get; init; }
    }

    public sealed record OllamaChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record OllamaChatResponse(
        [property: JsonPropertyName("message")] OllamaChatMessage? Message);
}

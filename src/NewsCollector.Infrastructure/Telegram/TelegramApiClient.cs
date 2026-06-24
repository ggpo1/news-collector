using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using NewsCollector.Application.Abstractions;

namespace NewsCollector.Infrastructure.Telegram;

public sealed class TelegramApiClient : ITelegramApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TelegramApiClient> _logger;

    public TelegramApiClient(IHttpClientFactory httpClientFactory, ILogger<TelegramApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<long> SendHtmlMessageAsync(
        string botToken,
        string chatId,
        string html,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await SendMessageAsync(botToken, chatId, html, "HTML", cancellationToken);
        }
        catch (InvalidOperationException ex) when (IsHtmlParseError(ex.Message))
        {
            _logger.LogWarning(
                "Telegram HTML parse failed for chat {ChatId}, retrying as plain text: {Error}",
                chatId,
                ex.Message);

            var plainText = TelegramMessageFormatter.ToPlainText(html);
            return await SendMessageAsync(botToken, chatId, plainText, parseMode: null, cancellationToken);
        }
    }

    public Task EnsureChatAccessibleAsync(
        string botToken,
        string chatId,
        CancellationToken cancellationToken = default) =>
        CallMethodAsync(botToken, "getChat", new Dictionary<string, object>
        {
            ["chat_id"] = ParseChatId(chatId)
        }, cancellationToken);

    private async Task<long> SendMessageAsync(
        string botToken,
        string chatId,
        string text,
        string? parseMode,
        CancellationToken cancellationToken)
    {
        var payload = new Dictionary<string, object>
        {
            ["chat_id"] = ParseChatId(chatId),
            ["text"] = text
        };

        if (!string.IsNullOrWhiteSpace(parseMode))
        {
            payload["parse_mode"] = parseMode;
        }

        var body = await CallMethodAsync(botToken, "sendMessage", payload, cancellationToken);
        if (!body.Result.TryGetProperty("message_id", out var messageIdProperty))
        {
            throw new InvalidOperationException("Telegram API returned success without message id");
        }

        return messageIdProperty.GetInt64();
    }

    private async Task<TelegramApiResponse> CallMethodAsync(
        string botToken,
        string method,
        Dictionary<string, object> payload,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("Telegram");
        var url = $"https://api.telegram.org/bot{botToken.Trim()}/{method}";

        using var response = await client.PostAsJsonAsync(url, payload, JsonOptions, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        TelegramApiResponse? body = null;
        try
        {
            body = JsonSerializer.Deserialize<TelegramApiResponse>(raw, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Telegram API returned non-JSON response for {Method}", method);
        }

        if (!response.IsSuccessStatusCode || body is null || !body.Ok)
        {
            var description = body?.Description
                ?? (string.IsNullOrWhiteSpace(raw) ? response.ReasonPhrase : raw)
                ?? "Unknown Telegram API error";

            _logger.LogWarning("Telegram {Method} failed for chat {ChatId}: {Error}", method, payload.GetValueOrDefault("chat_id"), description);
            throw new InvalidOperationException(description.Trim());
        }

        return body;
    }

    private static object ParseChatId(string chatId)
    {
        var trimmed = chatId.Trim();
        if (long.TryParse(trimmed, out var numericChatId))
        {
            return numericChatId;
        }

        return trimmed;
    }

    private static bool IsHtmlParseError(string message) =>
        message.Contains("can't parse entities", StringComparison.OrdinalIgnoreCase)
        || message.Contains("cant parse entities", StringComparison.OrdinalIgnoreCase)
        || message.Contains("parse entities", StringComparison.OrdinalIgnoreCase);

    private sealed class TelegramApiResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("result")]
        public JsonElement Result { get; set; }
    }
}

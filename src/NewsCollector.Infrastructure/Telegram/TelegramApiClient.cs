using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using NewsCollector.Application.Abstractions;

namespace NewsCollector.Infrastructure.Telegram;

public sealed class TelegramApiClient : ITelegramApiClient
{
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
        var client = _httpClientFactory.CreateClient("Telegram");
        var url = $"https://api.telegram.org/bot{botToken.Trim()}/sendMessage";

        using var response = await client.PostAsJsonAsync(
            url,
            new TelegramSendPayload(chatId.Trim(), html, "HTML"),
            cancellationToken);

        var body = await response.Content.ReadFromJsonAsync<TelegramApiResponse>(cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode || body is null || !body.Ok)
        {
            var description = body?.Description ?? response.ReasonPhrase ?? "Unknown Telegram API error";
            _logger.LogWarning("Telegram sendMessage failed for chat {ChatId}: {Error}", chatId, description);
            throw new InvalidOperationException(description);
        }

        return body.Result?.MessageId
            ?? throw new InvalidOperationException("Telegram API returned success without message id");
    }

    private sealed record TelegramSendPayload(
        [property: JsonPropertyName("chat_id")] string ChatId,
        [property: JsonPropertyName("text")] string Text,
        [property: JsonPropertyName("parse_mode")] string ParseMode);

    private sealed class TelegramApiResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("result")]
        public TelegramMessageResult? Result { get; set; }
    }

    private sealed class TelegramMessageResult
    {
        [JsonPropertyName("message_id")]
        public long MessageId { get; set; }
    }
}

namespace NewsCollector.Application.Abstractions;

public interface ITelegramApiClient
{
    Task<long> SendHtmlMessageAsync(string botToken, string chatId, string html, CancellationToken cancellationToken = default);
}

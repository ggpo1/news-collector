namespace NewsCollector.Application.Abstractions;

public interface IHtmlContentExtractor
{
    Task<string?> ExtractAsync(string html, string cssSelector, CancellationToken cancellationToken = default);
}

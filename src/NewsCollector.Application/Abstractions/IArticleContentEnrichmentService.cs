using NewsCollector.Application.Dtos;

namespace NewsCollector.Application.Abstractions;

public interface IArticleContentEnrichmentService
{
    Task<int> EnrichPendingArticlesAsync(CancellationToken cancellationToken = default);

    Task<ArticleEnrichmentResult> EnrichArticleAsync(Guid newsId, CancellationToken cancellationToken = default);
}

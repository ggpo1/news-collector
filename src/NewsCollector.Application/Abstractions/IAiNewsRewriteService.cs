using NewsCollector.Application.Dtos;

namespace NewsCollector.Application.Abstractions;

public interface IAiNewsRewriteService
{
    Task<AiNewsRewriteResultDto?> RewriteAsync(Guid newsId, CancellationToken cancellationToken = default);
}

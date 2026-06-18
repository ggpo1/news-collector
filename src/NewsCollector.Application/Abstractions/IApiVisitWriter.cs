using NewsCollector.Application.Models;

namespace NewsCollector.Application.Abstractions;

public interface IApiVisitWriter
{
    Task LogAsync(ApiVisitEntry entry, CancellationToken cancellationToken = default);
}

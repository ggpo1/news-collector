using NewsCollector.Application.Dtos;

namespace NewsCollector.Application.Abstractions;

public interface IEditorialDashboardService
{
    Task<EditorialDashboardDto> GetDashboardAsync(int? windowHours = null, CancellationToken cancellationToken = default);
}

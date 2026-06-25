using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Application.Abstractions;

public interface IEditorialBriefService
{
    Task<EditorialBriefReportDto?> GetLatestAsync(
        EditorialBriefPeriod? period = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EditorialBriefHistoryItemDto>> GetHistoryAsync(
        int limit = 14,
        CancellationToken cancellationToken = default);

    Task<EditorialBriefReportDto?> GenerateAsync(
        EditorialBriefPeriod period,
        CancellationToken cancellationToken = default);

    Task<int> TryGenerateScheduledAsync(CancellationToken cancellationToken = default);
}

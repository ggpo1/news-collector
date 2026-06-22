using NewsCollector.Application.Dtos;

namespace NewsCollector.Application.Abstractions;

public interface ISecondDayAngleService
{
    Task<SecondDayAnglesDto?> GenerateAsync(Guid newsId, CancellationToken cancellationToken = default);
}

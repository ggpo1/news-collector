using NewsCollector.Application.Dtos;

namespace NewsCollector.Application.Abstractions;

public interface ITelegramChannelService
{
    Task<IReadOnlyList<TelegramChannelDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TelegramChannelDto>> GetForNewsAsync(
        Guid? sourceId,
        Guid? categoryId,
        CancellationToken cancellationToken = default);

    Task<TelegramChannelDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TelegramChannelDto?> CreateAsync(CreateTelegramChannelRequest request, CancellationToken cancellationToken = default);

    Task<TelegramChannelDto?> UpdateAsync(Guid id, UpdateTelegramChannelRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

using NewsCollector.Application.Dtos;

namespace NewsCollector.Application.Abstractions;

public interface ITelegramBotService
{
    Task<IReadOnlyList<TelegramBotDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TelegramBotDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TelegramBotDto?> CreateAsync(CreateTelegramBotRequest request, CancellationToken cancellationToken = default);

    Task<TelegramBotDto?> UpdateAsync(Guid id, UpdateTelegramBotRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TelegramBotDto?> RestartContainerAsync(Guid id, CancellationToken cancellationToken = default);
}

using NewsCollector.Application.Dtos;

namespace NewsCollector.Application.Abstractions;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<UserDto?> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);

    Task<UserDeleteResult> DeleteAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default);
}

public enum UserDeleteResult
{
    NotFound,
    CannotDeleteSelf,
    HasRewrites,
    Deleted
}

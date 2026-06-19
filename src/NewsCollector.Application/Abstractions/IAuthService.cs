using NewsCollector.Application.Dtos;

namespace NewsCollector.Application.Abstractions;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    Task<CurrentUserDto?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}

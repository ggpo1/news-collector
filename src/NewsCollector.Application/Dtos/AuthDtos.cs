using NewsCollector.Domain.Enums;

namespace NewsCollector.Application.Dtos;

public sealed record LoginRequestDto(string Login, string Password);

public sealed record LoginResponseDto(string AccessToken, CurrentUserDto User);

public sealed record CurrentUserDto(
    Guid Id,
    string Login,
    string DisplayName,
    UserRole Role);

public sealed record UserDto(
    Guid Id,
    string Login,
    string DisplayName,
    UserRole Role,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateUserRequest(
    string Login,
    string Password,
    string DisplayName,
    UserRole Role);

public sealed record UpdateUserRequest(
    string DisplayName,
    UserRole Role,
    bool IsActive,
    string? Password);

public sealed record ValidateInvitationRequest(string Code);

public sealed record ValidateInvitationResponse(UserRole Role);

public sealed record RegisterRequest(
    string InvitationCode,
    string Login,
    string Password,
    string DisplayName);

public enum RegisterStatus
{
    Success,
    InvalidInvitation,
    LoginExists,
    InvalidInput
}

public sealed record RegisterResult(RegisterStatus Status, LoginResponseDto? Response);

public sealed record CreateInvitationCodeRequest(UserRole Role);

public sealed record InvitationCodeDto(
    Guid Code,
    UserRole Role,
    DateTimeOffset CreatedAt,
    Guid CreatedByUserId,
    string CreatedByLogin,
    DateTimeOffset? UsedAt,
    Guid? UsedByUserId,
    string? UsedByLogin);

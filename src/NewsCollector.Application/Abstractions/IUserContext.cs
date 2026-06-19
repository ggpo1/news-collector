using NewsCollector.Domain.Enums;

namespace NewsCollector.Application.Abstractions;

public interface IUserContext
{
    Guid? UserId { get; }

    string? Login { get; }

    UserRole? Role { get; }

    bool IsAuthenticated { get; }

    bool IsChiefEditor { get; }
}

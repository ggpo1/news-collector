using NewsCollector.Domain.Enums;

namespace NewsCollector.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public required string Login { get; set; }

    public required string PasswordHash { get; set; }

    public required string DisplayName { get; set; }

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<NewsRewrite> Rewrites { get; set; } = [];
}

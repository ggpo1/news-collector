using NewsCollector.Domain.Enums;

namespace NewsCollector.Domain.Entities;

public class InvitationCode
{
    public Guid Code { get; set; }

    public UserRole Role { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;

    public DateTimeOffset? UsedAt { get; set; }

    public Guid? UsedByUserId { get; set; }

    public User? UsedByUser { get; set; }
}

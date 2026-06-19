using Microsoft.EntityFrameworkCore;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Entities;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Auth;

public sealed class UserService : IUserService
{
    private readonly NewsCollectorDbContext _db;
    private readonly PasswordHasherService _passwordHasher;

    public UserService(NewsCollectorDbContext db, PasswordHasherService passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .AsNoTracking()
            .OrderBy(u => u.DisplayName)
            .Select(u => new UserDto(
                u.Id,
                u.Login,
                u.DisplayName,
                u.Role,
                u.IsActive,
                u.CreatedAt,
                u.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserDto?> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var login = request.Login.Trim().ToLowerInvariant();
        var displayName = request.DisplayName.Trim();

        if (await _db.Users.AnyAsync(u => u.Login == login, cancellationToken))
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = login,
            DisplayName = displayName,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        return new UserDto(
            user.Id,
            user.Login,
            user.DisplayName,
            user.Role,
            user.IsActive,
            user.CreatedAt,
            user.UpdatedAt);
    }

    public async Task<UserDto?> UpdateAsync(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user is null)
        {
            return null;
        }

        user.DisplayName = request.DisplayName.Trim();
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = _passwordHasher.Hash(request.Password);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new UserDto(
            user.Id,
            user.Login,
            user.DisplayName,
            user.Role,
            user.IsActive,
            user.CreatedAt,
            user.UpdatedAt);
    }

    public async Task<UserDeleteResult> DeleteAsync(
        Guid id,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        if (id == currentUserId)
        {
            return UserDeleteResult.CannotDeleteSelf;
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user is null)
        {
            return UserDeleteResult.NotFound;
        }

        if (await _db.NewsRewrites.AnyAsync(r => r.AuthorId == id, cancellationToken))
        {
            return UserDeleteResult.HasRewrites;
        }

        var createdCodes = await _db.InvitationCodes
            .Where(c => c.CreatedByUserId == id)
            .ToListAsync(cancellationToken);

        _db.InvitationCodes.RemoveRange(createdCodes);
        _db.Users.Remove(user);
        await _db.SaveChangesAsync(cancellationToken);

        return UserDeleteResult.Deleted;
    }
}

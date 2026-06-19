using Microsoft.EntityFrameworkCore;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Dtos;
using NewsCollector.Domain.Entities;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Auth;

public sealed class InvitationCodeService : IInvitationCodeService
{
    private readonly NewsCollectorDbContext _db;

    public InvitationCodeService(NewsCollectorDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<InvitationCodeDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _db.InvitationCodes
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new InvitationCodeDto(
                c.Code,
                c.Role,
                c.CreatedAt,
                c.CreatedByUserId,
                c.CreatedByUser.Login,
                c.UsedAt,
                c.UsedByUserId,
                c.UsedByUser != null ? c.UsedByUser.Login : null))
            .ToListAsync(cancellationToken);
    }

    public async Task<InvitationCodeDto?> CreateAsync(
        CreateInvitationCodeRequest request,
        Guid createdByUserId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var invitation = new InvitationCode
        {
            Code = Guid.NewGuid(),
            Role = request.Role,
            CreatedAt = now,
            CreatedByUserId = createdByUserId
        };

        _db.InvitationCodes.Add(invitation);
        await _db.SaveChangesAsync(cancellationToken);

        var creatorLogin = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == createdByUserId)
            .Select(u => u.Login)
            .FirstAsync(cancellationToken);

        return new InvitationCodeDto(
            invitation.Code,
            invitation.Role,
            invitation.CreatedAt,
            invitation.CreatedByUserId,
            creatorLogin,
            null,
            null,
            null);
    }

    public async Task<ValidateInvitationResponse?> ValidateAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (!TryParseCode(code, out var parsedCode))
        {
            return null;
        }

        var invitation = await _db.InvitationCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == parsedCode && c.UsedAt == null, cancellationToken);

        return invitation is null ? null : new ValidateInvitationResponse(invitation.Role);
    }

    public async Task<InvitationCodeDeleteResult> DeleteAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (!TryParseCode(code, out var parsedCode))
        {
            return InvitationCodeDeleteResult.InvalidCode;
        }

        var invitation = await _db.InvitationCodes
            .FirstOrDefaultAsync(c => c.Code == parsedCode, cancellationToken);

        if (invitation is null)
        {
            return InvitationCodeDeleteResult.NotFound;
        }

        _db.InvitationCodes.Remove(invitation);
        await _db.SaveChangesAsync(cancellationToken);

        return InvitationCodeDeleteResult.Deleted;
    }

    internal static bool TryParseCode(string code, out Guid parsedCode)
    {
        return Guid.TryParse(code.Trim(), out parsedCode);
    }
}

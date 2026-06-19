using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NewsCollector.Application.Options;
using NewsCollector.Domain.Entities;
using NewsCollector.Domain.Enums;
using NewsCollector.Infrastructure.Persistence;

namespace NewsCollector.Infrastructure.Auth;

public sealed class AuthDataSeeder
{
    private readonly NewsCollectorDbContext _db;
    private readonly PasswordHasherService _passwordHasher;
    private readonly AuthOptions _options;
    private readonly ILogger<AuthDataSeeder> _logger;

    public AuthDataSeeder(
        NewsCollectorDbContext db,
        PasswordHasherService passwordHasher,
        IOptions<AuthOptions> options,
        ILogger<AuthDataSeeder> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        User? chiefEditor = null;

        if (!await _db.Users.AnyAsync(cancellationToken))
        {
            var now = DateTimeOffset.UtcNow;
            chiefEditor = new User
            {
                Id = Guid.NewGuid(),
                Login = _options.SeedChiefEditorLogin.Trim().ToLowerInvariant(),
                DisplayName = _options.SeedChiefEditorDisplayName.Trim(),
                PasswordHash = _passwordHasher.Hash(_options.SeedChiefEditorPassword),
                Role = UserRole.ChiefEditor,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.Users.Add(chiefEditor);
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Created seed chief editor account '{Login}'",
                chiefEditor.Login);
        }
        else
        {
            chiefEditor = await _db.Users
                .Where(u => u.Role == UserRole.ChiefEditor && u.IsActive)
                .OrderBy(u => u.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (chiefEditor is null)
        {
            return;
        }

        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE news_rewrites
            SET "AuthorId" = {chiefEditor.Id}
            WHERE "AuthorId" IS NULL OR "AuthorId" = {Guid.Empty}
            """,
            cancellationToken);

        var orphanedRewrites = await _db.NewsRewrites
            .Where(r => r.AuthorId == Guid.Empty)
            .ToListAsync(cancellationToken);

        if (orphanedRewrites.Count == 0)
        {
            return;
        }

        foreach (var rewrite in orphanedRewrites)
        {
            rewrite.AuthorId = chiefEditor.Id;
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Assigned {Count} legacy rewrites to chief editor {Login}",
            orphanedRewrites.Count,
            chiefEditor.Login);
    }
}

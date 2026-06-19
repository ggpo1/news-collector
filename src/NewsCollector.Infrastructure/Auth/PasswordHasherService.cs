using Microsoft.AspNetCore.Identity;

namespace NewsCollector.Infrastructure.Auth;

public sealed class PasswordHasherService
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password) =>
        _hasher.HashPassword(null!, password);

    public bool Verify(string password, string passwordHash) =>
        _hasher.VerifyHashedPassword(null!, passwordHash, password) != PasswordVerificationResult.Failed;
}

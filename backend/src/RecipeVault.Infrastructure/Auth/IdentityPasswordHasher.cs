using Microsoft.AspNetCore.Identity;
using RecipeVault.Application.Auth;

namespace RecipeVault.Infrastructure.Auth;

/// <summary>
/// Uses ASP.NET Core Identity's hasher (PBKDF2 with a per-password salt) without
/// pulling in the rest of Identity. See docs/adr/0006.
/// </summary>
public sealed class IdentityPasswordHasher : IPasswordHasher
{
    private static readonly PasswordHasher<object> Hasher = new();
    private static readonly object Subject = new();

    public string Hash(string password) => Hasher.HashPassword(Subject, password);

    public bool Verify(string passwordHash, string password) =>
        Hasher.VerifyHashedPassword(Subject, passwordHash, password) != PasswordVerificationResult.Failed;
}

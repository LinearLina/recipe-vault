namespace RecipeVault.Application.Auth;

public sealed class CredentialsRequest
{
    public string? Email { get; init; }
    public string? Password { get; init; }
}

public sealed record SignupResponse(int Id, string Email);

public sealed record LoginResponse(string Token);

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string passwordHash, string password);
}

public interface ITokenService
{
    string CreateToken(RecipeVault.Domain.User user);
}

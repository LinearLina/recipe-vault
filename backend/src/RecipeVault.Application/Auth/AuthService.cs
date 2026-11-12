using Microsoft.EntityFrameworkCore;
using RecipeVault.Application.Common;
using RecipeVault.Domain;

namespace RecipeVault.Application.Auth;

public interface IAuthService
{
    Task<Result<SignupResponse>> SignupAsync(CredentialsRequest request, CancellationToken ct);
    Task<Result<LoginResponse>> LoginAsync(CredentialsRequest request, CancellationToken ct);
}

public sealed class AuthService(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    TimeProvider clock) : IAuthService
{
    public const int MinPasswordLength = 8;
    public const string InvalidCredentialsMessage = "Invalid email or password";

    public async Task<Result<SignupResponse>> SignupAsync(CredentialsRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add("email is required");
        }
        else if (!request.Email.Contains('@', StringComparison.Ordinal) || request.Email.Trim().Length > User.EmailMaxLength)
        {
            errors.Add("email is not valid");
        }

        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < MinPasswordLength)
        {
            errors.Add($"password must be at least {MinPasswordLength} characters");
        }

        if (errors.Count > 0)
        {
            return Result<SignupResponse>.Invalid(errors);
        }

        var email = User.NormalizeEmail(request.Email!);
        if (await db.Users.AnyAsync(u => u.Email == email, ct))
        {
            return Result<SignupResponse>.Conflict("Email already in use");
        }

        var user = new User(email, passwordHasher.Hash(request.Password!), clock.GetUtcNow());
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return Result<SignupResponse>.Ok(new SignupResponse(user.Id, user.Email));
    }

    public async Task<Result<LoginResponse>> LoginAsync(CredentialsRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return Result<LoginResponse>.Invalid(["email and password are required"]);
        }

        var email = User.NormalizeEmail(request.Email);
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);

        // Same message whether the email or the password is wrong, so the API doesn't reveal which emails exist.
        if (user is null || !passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            return Result<LoginResponse>.Unauthorized(InvalidCredentialsMessage);
        }

        return Result<LoginResponse>.Ok(new LoginResponse(tokenService.CreateToken(user)));
    }
}

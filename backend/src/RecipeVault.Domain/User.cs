namespace RecipeVault.Domain;

public class User
{
    public const int EmailMaxLength = 320;

    private User()
    {
    } // for EF Core

    public User(string email, string passwordHash, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainValidationException("email is required");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainValidationException("password hash is required");
        }

        Email = NormalizeEmail(email);
        PasswordHash = passwordHash;
        CreatedAt = createdAt;
    }

    public int Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Emails are compared case-insensitively, so they are stored in one canonical form.</summary>
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}

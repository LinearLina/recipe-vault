namespace RecipeVault.Api.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>HMAC signing key. Never commit a real one: use user-secrets locally and an env var in production.</summary>
    public string Secret { get; init; } = string.Empty;
    public string Issuer { get; init; } = "recipe-vault";
    public string Audience { get; init; } = "recipe-vault";
    public int ExpiryDays { get; init; } = 7;
}

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RecipeVault.Api.Errors;
using RecipeVault.Application.Auth;

namespace RecipeVault.Api.Auth;

public static class AuthenticationSetup
{
    private const int MinSecretBytes = 32; // HMAC-SHA256 needs a 256-bit key

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        if (Encoding.UTF8.GetByteCount(jwt.Secret) < MinSecretBytes)
        {
            throw new InvalidOperationException(
                $"Jwt:Secret must be at least {MinSecretBytes} bytes. " +
                "Set it with 'dotnet user-secrets set Jwt:Secret <value>' or the Jwt__Secret environment variable.");
        }

        services.AddSingleton(jwt);
        services.AddSingleton<ITokenService, JwtTokenService>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false; // keep "sub" as "sub"
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                };
                options.Events = new JwtBearerEvents
                {
                    // Return JSON the client can show, instead of an empty 401.
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new ApiError("Please log in to do that"));
                    },
                };
            });

        services.AddAuthorization();
        return services;
    }
}

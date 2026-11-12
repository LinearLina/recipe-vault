using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeVault.Application.Auth;
using RecipeVault.Application.Common;
using RecipeVault.Infrastructure.Auth;
using RecipeVault.Infrastructure.Persistence;

namespace RecipeVault.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                // Azure SQL serverless pauses when idle; retry while it wakes up.
                sql.EnableRetryOnFailure();
                // Loading a recipe with ingredients and tags in one JOIN multiplies rows; split it instead.
                sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            }));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();

        return services;
    }
}

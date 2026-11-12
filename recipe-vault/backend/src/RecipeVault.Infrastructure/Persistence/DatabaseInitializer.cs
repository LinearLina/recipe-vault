using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RecipeVault.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    /// <summary>Applies pending EF Core migrations and optionally loads sample data.</summary>
    public static async Task MigrateAsync(IServiceProvider services, bool seed, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync(ct);

        if (seed)
        {
            await DbSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<TimeProvider>(), ct);
        }
    }
}

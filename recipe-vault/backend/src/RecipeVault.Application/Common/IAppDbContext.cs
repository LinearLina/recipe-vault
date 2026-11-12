using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain;

namespace RecipeVault.Application.Common;

/// <summary>
/// The slice of the database the application layer is allowed to use.
/// See docs/adr/0002 for why this exposes EF Core DbSets rather than custom repositories.
/// </summary>
public interface IAppDbContext
{
    DbSet<Recipe> Recipes { get; }
    DbSet<Tag> Tags { get; }
    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

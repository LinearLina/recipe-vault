using Microsoft.EntityFrameworkCore;
using RecipeVault.Domain;

namespace RecipeVault.Infrastructure.Persistence;

/// <summary>Sample data for local development and E2E tests. Only runs against an empty database.</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, TimeProvider clock, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(clock);

        if (await db.Recipes.AnyAsync(ct))
        {
            return;
        }

        var now = clock.GetUtcNow();
        var tags = new[] { "vegetarian", "quick", "dessert", "italian", "spicy" }
            .ToDictionary(name => name, name => new Tag(name));

        var noodles = new Recipe(
            userId: null,
            new RecipeDetails(
                "Fifteen-Minute Garlic Noodles",
                "A weeknight staple: salty, garlicky, done before the kettle cools.",
                "1. Boil noodles.\n2. Fry garlic in butter and soy sauce.\n3. Toss together.",
                PrepMinutes: 5,
                CookMinutes: 10,
                Servings: 2,
                ImageUrl: null),
            now);
        noodles.ReplaceIngredients(
        [
            new IngredientLine("egg noodles", 200, "g"),
            new IngredientLine("garlic cloves", 6, null),
            new IngredientLine("soy sauce", 3, "tbsp"),
            new IngredientLine("butter", 2, "tbsp"),
        ]);
        noodles.SetTags([tags["quick"], tags["vegetarian"]]);

        var tiramisu = new Recipe(
            userId: null,
            new RecipeDetails(
                "Classic Tiramisu",
                "No-bake Italian dessert, layered and chilled.",
                "1. Whisk mascarpone mixture.\n2. Dip ladyfingers in coffee.\n3. Layer and chill overnight.",
                PrepMinutes: 30,
                CookMinutes: 0,
                Servings: 8,
                ImageUrl: null),
            now);
        tiramisu.ReplaceIngredients(
        [
            new IngredientLine("mascarpone", 500, "g"),
            new IngredientLine("ladyfingers", 24, null),
            new IngredientLine("espresso", 300, "ml"),
            new IngredientLine("cocoa powder", 2, "tbsp"),
        ]);
        tiramisu.SetTags([tags["dessert"], tags["italian"]]);

        db.Tags.AddRange(tags.Values);
        db.Recipes.AddRange(noodles, tiramisu);
        await db.SaveChangesAsync(ct);
    }
}

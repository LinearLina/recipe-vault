using Microsoft.EntityFrameworkCore;
using RecipeVault.Application.Common;
using RecipeVault.Domain;

namespace RecipeVault.Application.Recipes;

public interface IRecipeService
{
    Task<IReadOnlyList<RecipeSummaryDto>> ListAsync(string? search, string? tag, CancellationToken ct);
    Task<Result<RecipeDetailDto>> GetAsync(int id, CancellationToken ct);
    Task<Result<ScaledRecipeDto>> ScaleAsync(int id, int servings, CancellationToken ct);
    Task<Result<RecipeIdDto>> CreateAsync(int userId, RecipeRequest request, CancellationToken ct);
    Task<Result<RecipeIdDto>> UpdateAsync(int userId, int id, RecipeRequest request, CancellationToken ct);
    Task<Result<RecipeIdDto>> DeleteAsync(int userId, int id, CancellationToken ct);
}

public sealed class RecipeService(IAppDbContext db, TimeProvider clock) : IRecipeService
{
    public const string NotFoundMessage = "Recipe not found";
    public const string NotOwnerMessage = "You can only change your own recipes";

    public async Task<IReadOnlyList<RecipeSummaryDto>> ListAsync(string? search, string? tag, CancellationToken ct)
    {
        var query = db.Recipes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(r => r.Title.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var tagName = tag.Trim();
            query = query.Where(r => r.Tags.Any(t => t.Name == tagName));
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .Select(r => new RecipeSummaryDto(
                r.Id,
                r.Title,
                r.Description,
                r.PrepMinutes,
                r.CookMinutes,
                r.Servings,
                r.ImageUrl,
                r.CreatedAt,
                r.Tags.OrderBy(t => t.Name).Select(t => t.Name).ToList()))
            .ToListAsync(ct);
    }

    public async Task<Result<RecipeDetailDto>> GetAsync(int id, CancellationToken ct)
    {
        var recipe = await LoadWithRelations(db.Recipes.AsNoTracking(), id, ct);

        return recipe is null
            ? Result<RecipeDetailDto>.NotFound(NotFoundMessage)
            : Result<RecipeDetailDto>.Ok(ToDetail(recipe));
    }

    public async Task<Result<ScaledRecipeDto>> ScaleAsync(int id, int servings, CancellationToken ct)
    {
        if (servings < 1)
        {
            return Result<ScaledRecipeDto>.Invalid(["servings must be at least 1"]);
        }

        var recipe = await db.Recipes.AsNoTracking()
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (recipe is null)
        {
            return Result<ScaledRecipeDto>.NotFound(NotFoundMessage);
        }

        var scaled = recipe.ScaleIngredients(servings)
            .Select(line => new ScaledIngredientDto(line.Name, line.Amount, line.Unit))
            .ToList();

        return Result<ScaledRecipeDto>.Ok(new ScaledRecipeDto(recipe.Id, recipe.Title, servings, scaled));
    }

    public async Task<Result<RecipeIdDto>> CreateAsync(int userId, RecipeRequest request, CancellationToken ct)
    {
        var errors = RecipeRequestValidator.Validate(request);
        if (errors.Count > 0)
        {
            return Result<RecipeIdDto>.Invalid(errors);
        }

        var recipe = new Recipe(userId, ToDetails(request), clock.GetUtcNow());
        recipe.ReplaceIngredients(ToLines(request));
        recipe.SetTags(await ResolveTagsAsync(request.Tags, ct));

        db.Recipes.Add(recipe);
        await db.SaveChangesAsync(ct); // one transaction: recipe, ingredients and tags succeed or fail together

        return Result<RecipeIdDto>.Ok(new RecipeIdDto(recipe.Id));
    }

    public async Task<Result<RecipeIdDto>> UpdateAsync(int userId, int id, RecipeRequest request, CancellationToken ct)
    {
        var errors = RecipeRequestValidator.Validate(request);
        if (errors.Count > 0)
        {
            return Result<RecipeIdDto>.Invalid(errors);
        }

        var recipe = await LoadWithRelations(db.Recipes, id, ct);
        if (recipe is null)
        {
            return Result<RecipeIdDto>.NotFound(NotFoundMessage);
        }

        if (!recipe.IsOwnedBy(userId))
        {
            return Result<RecipeIdDto>.Forbidden(NotOwnerMessage);
        }

        recipe.Update(ToDetails(request), clock.GetUtcNow());
        recipe.ReplaceIngredients(ToLines(request));
        recipe.SetTags(await ResolveTagsAsync(request.Tags, ct));

        await db.SaveChangesAsync(ct);

        return Result<RecipeIdDto>.Ok(new RecipeIdDto(recipe.Id));
    }

    public async Task<Result<RecipeIdDto>> DeleteAsync(int userId, int id, CancellationToken ct)
    {
        var recipe = await db.Recipes.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (recipe is null)
        {
            return Result<RecipeIdDto>.NotFound(NotFoundMessage);
        }

        if (!recipe.IsOwnedBy(userId))
        {
            return Result<RecipeIdDto>.Forbidden(NotOwnerMessage);
        }

        db.Recipes.Remove(recipe); // ingredients and tag links are removed by cascade delete
        await db.SaveChangesAsync(ct);

        return Result<RecipeIdDto>.Ok(new RecipeIdDto(id));
    }

    private static Task<Recipe?> LoadWithRelations(IQueryable<Recipe> source, int id, CancellationToken ct) =>
        source
            .Include(r => r.Ingredients)
            .Include(r => r.Tags) // split into separate queries globally, see Infrastructure/DependencyInjection.cs
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    /// <summary>Finds existing tags by name and creates any that don't exist yet.</summary>
    private async Task<List<Tag>> ResolveTagsAsync(IEnumerable<string>? names, CancellationToken ct)
    {
        var wanted = (names ?? Enumerable.Empty<string>())
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (wanted.Count == 0)
        {
            return [];
        }

        var existing = await db.Tags.Where(t => wanted.Contains(t.Name)).ToListAsync(ct);
        var result = new List<Tag>(existing);

        foreach (var name in wanted)
        {
            if (!existing.Exists(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                result.Add(new Tag(name));
            }
        }

        return result;
    }

    // Validation has already run, so Title and Instructions are known to be present here.
    private static RecipeDetails ToDetails(RecipeRequest r) => new(
        r.Title!,
        r.Description,
        r.Instructions!,
        r.PrepMinutes ?? 0,
        r.CookMinutes ?? 0,
        r.Servings ?? 1,
        r.ImageUrl);

    private static IEnumerable<IngredientLine> ToLines(RecipeRequest r) =>
        (r.Ingredients ?? []).Select(i => new IngredientLine(i.Name!, i.Amount, i.Unit));

    private static RecipeDetailDto ToDetail(Recipe r) => new(
        r.Id,
        r.UserId,
        r.Title,
        r.Description,
        r.Instructions,
        r.PrepMinutes,
        r.CookMinutes,
        r.Servings,
        r.ImageUrl,
        r.CreatedAt,
        r.UpdatedAt,
        r.Ingredients.OrderBy(i => i.Position).Select(i => new IngredientDto(i.Id, i.Name, i.Amount, i.Unit)).ToList(),
        r.Tags.Select(t => t.Name).Order(StringComparer.OrdinalIgnoreCase).ToList());
}

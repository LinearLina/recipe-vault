namespace RecipeVault.Application.Recipes;

// Request types use nullable properties so missing fields reach the validator
// and produce the same messages the original Express API returned.

public sealed class RecipeRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Instructions { get; init; }
    public int? PrepMinutes { get; init; }
    public int? CookMinutes { get; init; }
    public int? Servings { get; init; }
    public string? ImageUrl { get; init; }
    public IReadOnlyList<IngredientRequest>? Ingredients { get; init; }
    public IReadOnlyList<string>? Tags { get; init; }
}

public sealed class IngredientRequest
{
    public string? Name { get; init; }
    public decimal? Amount { get; init; }
    public string? Unit { get; init; }
}

public sealed record RecipeSummaryDto(
    int Id,
    string Title,
    string? Description,
    int PrepMinutes,
    int CookMinutes,
    int Servings,
    string? ImageUrl,
    DateTimeOffset CreatedAt,
    IReadOnlyList<string> Tags);

public sealed record IngredientDto(int Id, string Name, decimal? Amount, string? Unit);

public sealed record RecipeDetailDto(
    int Id,
    int? UserId,
    string Title,
    string? Description,
    string Instructions,
    int PrepMinutes,
    int CookMinutes,
    int Servings,
    string? ImageUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<IngredientDto> Ingredients,
    IReadOnlyList<string> Tags);

public sealed record ScaledIngredientDto(string Name, decimal? Amount, string? Unit);

public sealed record ScaledRecipeDto(int Id, string Title, int Servings, IReadOnlyList<ScaledIngredientDto> Ingredients);

public sealed record RecipeIdDto(int Id);

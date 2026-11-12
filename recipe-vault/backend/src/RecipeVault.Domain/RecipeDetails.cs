namespace RecipeVault.Domain;

/// <summary>The editable fields of a recipe, grouped so create and update share one validation path.</summary>
public sealed record RecipeDetails(
    string Title,
    string? Description,
    string Instructions,
    int PrepMinutes,
    int CookMinutes,
    int Servings,
    string? ImageUrl);

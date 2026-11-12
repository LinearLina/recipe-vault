namespace RecipeVault.Domain;

/// <summary>An ingredient as written in a recipe, independent of how it is stored.</summary>
public sealed record IngredientLine(string Name, decimal? Amount, string? Unit);

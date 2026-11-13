using RecipeVault.Domain;

namespace RecipeVault.Application.Recipes;

/// <summary>
/// Turns a recipe request into a list of user-facing error messages.
/// Returns every problem at once so the form can show them together.
/// </summary>
public static class RecipeRequestValidator
{
    public static IReadOnlyList<string> Validate(RecipeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors.Add("title is required");
        }
        else if (request.Title.Trim().Length > Recipe.TitleMaxLength)
        {
            errors.Add($"title must be {Recipe.TitleMaxLength} characters or fewer");
        }

        if (string.IsNullOrWhiteSpace(request.Instructions))
        {
            errors.Add("instructions is required");
        }

        if (request.Description?.Length > Recipe.DescriptionMaxLength)
        {
            errors.Add($"description must be {Recipe.DescriptionMaxLength} characters or fewer");
        }

        if (request.ImageUrl?.Length > Recipe.ImageUrlMaxLength)
        {
            errors.Add($"image_url must be {Recipe.ImageUrlMaxLength} characters or fewer");
        }

        if (request.PrepMinutes < 0)
        {
            errors.Add("prep_minutes cannot be negative");
        }

        if (request.CookMinutes < 0)
        {
            errors.Add("cook_minutes cannot be negative");
        }

        if (request.Servings < 1)
        {
            errors.Add("servings must be at least 1");
        }

        var ingredients = request.Ingredients ?? [];

        if (ingredients.Any(i => string.IsNullOrWhiteSpace(i.Name)))
        {
            errors.Add("every ingredient needs a name");
        }

        if (ingredients.Any(i => i.Name?.Trim().Length > Ingredient.NameMaxLength))
        {
            errors.Add($"ingredient names must be {Ingredient.NameMaxLength} characters or fewer");
        }

        if (ingredients.Any(i => i.Unit?.Trim().Length > Ingredient.UnitMaxLength))
        {
            errors.Add($"ingredient units must be {Ingredient.UnitMaxLength} characters or fewer");
        }

        if (ingredients.Any(i => i.Amount < 0))
        {
            errors.Add("ingredient amounts cannot be negative");
        }

        if ((request.Tags ?? []).Any(t => t?.Trim().Length > Tag.NameMaxLength))
        {
            errors.Add($"tags must be {Tag.NameMaxLength} characters or fewer");
        }

        return errors;
    }
}

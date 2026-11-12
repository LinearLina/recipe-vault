using RecipeVault.Application.Recipes;

namespace RecipeVault.UnitTests;

public class RecipeRequestValidatorTests
{
    private static RecipeRequest Valid() => new()
    {
        Title = "Pancakes",
        Instructions = "Mix, cook, flip.",
        Servings = 2,
        Ingredients = [new IngredientRequest { Name = "flour", Amount = 200, Unit = "g" }],
        Tags = ["breakfast"],
    };

    [Fact]
    public void A_complete_request_has_no_errors() =>
        Assert.Empty(RecipeRequestValidator.Validate(Valid()));

    [Fact]
    public void Optional_fields_can_be_left_out() =>
        Assert.Empty(RecipeRequestValidator.Validate(new RecipeRequest { Title = "Toast", Instructions = "Toast it." }));

    [Fact]
    public void Missing_title_and_instructions_are_both_reported()
    {
        var errors = RecipeRequestValidator.Validate(new RecipeRequest());

        // Same messages the original Express API returned, so the client shows the same text.
        Assert.Contains("title is required", errors);
        Assert.Contains("instructions is required", errors);
    }

    [Fact]
    public void Overlong_title_is_rejected()
    {
        var request = new RecipeRequest { Title = new string('a', 201), Instructions = "x" };

        Assert.Contains("title must be 200 characters or fewer", RecipeRequestValidator.Validate(request));
    }

    [Fact]
    public void Negative_minutes_and_zero_servings_are_rejected()
    {
        var request = new RecipeRequest { Title = "x", Instructions = "x", PrepMinutes = -1, CookMinutes = -5, Servings = 0 };

        var errors = RecipeRequestValidator.Validate(request);

        Assert.Contains("prep_minutes cannot be negative", errors);
        Assert.Contains("cook_minutes cannot be negative", errors);
        Assert.Contains("servings must be at least 1", errors);
    }

    [Fact]
    public void Ingredient_without_a_name_is_rejected()
    {
        var request = new RecipeRequest
        {
            Title = "x",
            Instructions = "x",
            Ingredients = [new IngredientRequest { Name = " ", Amount = 1 }],
        };

        Assert.Contains("every ingredient needs a name", RecipeRequestValidator.Validate(request));
    }

    [Fact]
    public void Negative_ingredient_amount_is_rejected()
    {
        var request = new RecipeRequest
        {
            Title = "x",
            Instructions = "x",
            Ingredients = [new IngredientRequest { Name = "flour", Amount = -3 }],
        };

        Assert.Contains("ingredient amounts cannot be negative", RecipeRequestValidator.Validate(request));
    }

    [Fact]
    public void Overlong_tag_is_rejected()
    {
        var request = new RecipeRequest { Title = "x", Instructions = "x", Tags = [new string('t', 51)] };

        Assert.Contains("tags must be 50 characters or fewer", RecipeRequestValidator.Validate(request));
    }
}

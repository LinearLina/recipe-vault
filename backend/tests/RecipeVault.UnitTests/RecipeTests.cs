using RecipeVault.Domain;

namespace RecipeVault.UnitTests;

public class RecipeTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 12, 9, 0, 0, TimeSpan.Zero);

    private static RecipeDetails Details(
        string title = "Pancakes",
        string instructions = "Mix, cook, flip.",
        int servings = 4,
        int prep = 5,
        int cook = 10) =>
        new(title, "Fluffy", instructions, prep, cook, servings, ImageUrl: null);

    [Fact]
    public void Constructor_sets_fields_and_timestamps()
    {
        var recipe = new Recipe(userId: 7, Details(title: "  Pancakes  "), Now);

        Assert.Equal("Pancakes", recipe.Title);
        Assert.Equal(7, recipe.UserId);
        Assert.Equal(Now, recipe.CreatedAt);
        Assert.Equal(Now, recipe.UpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_blank_title(string title)
    {
        var ex = Assert.Throws<DomainValidationException>(() => new Recipe(1, Details(title: title), Now));
        Assert.Equal("title is required", ex.Message);
    }

    [Fact]
    public void Constructor_rejects_blank_instructions()
    {
        var ex = Assert.Throws<DomainValidationException>(() => new Recipe(1, Details(instructions: " "), Now));
        Assert.Equal("instructions is required", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void Constructor_rejects_servings_below_one(int servings) =>
        Assert.Throws<DomainValidationException>(() => new Recipe(1, Details(servings: servings), Now));

    [Fact]
    public void Constructor_rejects_negative_minutes() =>
        Assert.Throws<DomainValidationException>(() => new Recipe(1, Details(prep: -1), Now));

    [Fact]
    public void Update_changes_fields_and_bumps_UpdatedAt_but_not_CreatedAt()
    {
        var recipe = new Recipe(1, Details(), Now);
        var later = Now.AddHours(2);

        recipe.Update(Details(title: "Better pancakes", servings: 2), later);

        Assert.Equal("Better pancakes", recipe.Title);
        Assert.Equal(2, recipe.Servings);
        Assert.Equal(Now, recipe.CreatedAt);
        Assert.Equal(later, recipe.UpdatedAt);
    }

    [Fact]
    public void ReplaceIngredients_keeps_the_given_order()
    {
        var recipe = new Recipe(1, Details(), Now);

        recipe.ReplaceIngredients(
        [
            new IngredientLine("flour", 200, "g"),
            new IngredientLine("egg", 2, null),
            new IngredientLine("milk", 300, "ml"),
        ]);

        Assert.Equal(["flour", "egg", "milk"], recipe.Ingredients.Select(i => i.Name));
        Assert.Equal([0, 1, 2], recipe.Ingredients.Select(i => i.Position));
    }

    [Fact]
    public void ReplaceIngredients_removes_ingredients_that_are_no_longer_listed()
    {
        var recipe = new Recipe(1, Details(), Now);
        recipe.ReplaceIngredients([new IngredientLine("flour", 200, "g"), new IngredientLine("sugar", 50, "g")]);

        recipe.ReplaceIngredients([new IngredientLine("flour", 250, "g")]);

        var only = Assert.Single(recipe.Ingredients);
        Assert.Equal(250m, only.Amount);
    }

    [Fact]
    public void ReplaceIngredients_leaves_the_recipe_unchanged_when_a_line_is_invalid()
    {
        var recipe = new Recipe(1, Details(), Now);
        recipe.ReplaceIngredients([new IngredientLine("flour", 200, "g")]);

        Assert.Throws<DomainValidationException>(() =>
            recipe.ReplaceIngredients([new IngredientLine("sugar", 50, "g"), new IngredientLine(" ", 1, null)]));

        Assert.Equal("flour", Assert.Single(recipe.Ingredients).Name);
    }

    [Fact]
    public void ReplaceIngredients_rejects_negative_amounts() =>
        Assert.Throws<DomainValidationException>(() =>
            new Recipe(1, Details(), Now).ReplaceIngredients([new IngredientLine("flour", -1, "g")]));

    [Fact]
    public void ReplaceIngredients_stores_blank_units_as_null()
    {
        var recipe = new Recipe(1, Details(), Now);
        recipe.ReplaceIngredients([new IngredientLine("egg", 2, "  ")]);

        Assert.Null(recipe.Ingredients[0].Unit);
    }

    [Fact]
    public void SetTags_ignores_duplicates_regardless_of_case()
    {
        var recipe = new Recipe(1, Details(), Now);

        recipe.SetTags([new Tag("Quick"), new Tag("quick"), new Tag("dessert")]);

        Assert.Equal(2, recipe.Tags.Count);
    }

    [Fact]
    public void SetTags_keeps_existing_tag_objects_and_only_applies_differences()
    {
        var quick = new Tag("quick");
        var dessert = new Tag("dessert");
        var recipe = new Recipe(1, Details(), Now);
        recipe.SetTags([quick, dessert]);

        recipe.SetTags([new Tag("QUICK"), new Tag("vegan")]);

        Assert.Contains(quick, recipe.Tags); // same instance kept, so EF doesn't delete and re-add the link
        Assert.DoesNotContain(dessert, recipe.Tags);
        Assert.Contains(recipe.Tags, t => t.Name == "vegan");
    }

    [Fact]
    public void ScaleIngredients_doubles_amounts_when_servings_double()
    {
        var recipe = new Recipe(1, Details(servings: 4), Now);
        recipe.ReplaceIngredients([new IngredientLine("flour", 200, "g"), new IngredientLine("egg", 2, null)]);

        var scaled = recipe.ScaleIngredients(8);

        Assert.Equal([400m, 4m], scaled.Select(i => i.Amount));
    }

    [Fact]
    public void ScaleIngredients_rounds_to_two_decimal_places()
    {
        var recipe = new Recipe(1, Details(servings: 3), Now);
        recipe.ReplaceIngredients([new IngredientLine("butter", 100, "g")]);

        var scaled = recipe.ScaleIngredients(1);

        Assert.Equal(33.33m, scaled[0].Amount);
    }

    [Fact]
    public void ScaleIngredients_leaves_missing_amounts_missing()
    {
        var recipe = new Recipe(1, Details(servings: 2), Now);
        recipe.ReplaceIngredients([new IngredientLine("salt", null, "to taste")]);

        Assert.Null(recipe.ScaleIngredients(6)[0].Amount);
    }

    [Fact]
    public void ScaleIngredients_rejects_zero_servings() =>
        Assert.Throws<DomainValidationException>(() => new Recipe(1, Details(), Now).ScaleIngredients(0));

    [Fact]
    public void IsOwnedBy_is_false_for_sample_recipes_without_an_owner()
    {
        var sample = new Recipe(userId: null, Details(), Now);

        Assert.False(sample.IsOwnedBy(1));
    }
}

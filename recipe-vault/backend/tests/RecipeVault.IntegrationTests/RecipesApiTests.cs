using System.Net;
using System.Net.Http.Json;
using RecipeVault.Application.Recipes;

namespace RecipeVault.IntegrationTests;

public sealed class RecipesApiTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Health_reports_ok_when_the_database_is_reachable()
    {
        var response = await Client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("ok", (await ReadJsonAsync(response)).GetProperty("status").GetString());
    }

    [Fact]
    public async Task Creating_a_recipe_requires_login()
    {
        var response = await Client.PostAsJsonAsync("/api/recipes", PancakeRequest(), Json);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.True((await ReadJsonAsync(response)).TryGetProperty("error", out _));
    }

    [Fact]
    public async Task Creating_a_recipe_without_title_or_instructions_returns_all_validation_errors()
    {
        await LogInAsNewUserAsync();

        var response = await Client.PostAsJsonAsync("/api/recipes", new { description = "no title" }, Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await ReadJsonAsync(response)).GetProperty("errors").EnumerateArray().Select(e => e.GetString()).ToList();
        Assert.Contains("title is required", errors);
        Assert.Contains("instructions is required", errors);
    }

    [Fact]
    public async Task Malformed_json_returns_errors_in_the_same_shape()
    {
        await LogInAsNewUserAsync();

        var response = await Client.PostAsJsonAsync("/api/recipes", new { title = "x", instructions = "y", ingredients = "not a list" }, Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(System.Text.Json.JsonValueKind.Array, (await ReadJsonAsync(response)).GetProperty("errors").ValueKind);
    }

    [Fact]
    public async Task A_created_recipe_comes_back_with_its_ingredients_in_order_and_its_tags()
    {
        await LogInAsNewUserAsync();

        var id = await CreateRecipeAsync(PancakeRequest());
        var recipe = await GetRecipeAsync(id);

        Assert.Equal("Test Pancakes", recipe.Title);
        Assert.Equal(["flour", "egg"], recipe.Ingredients.Select(i => i.Name));
        Assert.Equal(200m, recipe.Ingredients[0].Amount);
        Assert.Null(recipe.Ingredients[1].Unit);
        Assert.Equal(["breakfast", "quick"], recipe.Tags);
        Assert.NotNull(recipe.UserId);
    }

    [Fact]
    public async Task Responses_use_snake_case_so_the_existing_client_keeps_working()
    {
        await LogInAsNewUserAsync();
        var id = await CreateRecipeAsync(PancakeRequest());

        var json = await ReadJsonAsync(await Client.GetAsync($"/api/recipes/{id}"));

        Assert.True(json.TryGetProperty("prep_minutes", out _));
        Assert.True(json.TryGetProperty("cook_minutes", out _));
        Assert.True(json.TryGetProperty("image_url", out _));
    }

    [Fact]
    public async Task List_filters_by_tag_and_by_title_search()
    {
        await LogInAsNewUserAsync();
        var pancakesId = await CreateRecipeAsync(PancakeRequest("Test Pancakes", "breakfast"));
        var curryId = await CreateRecipeAsync(PancakeRequest("Weeknight Curry", "dinner"));

        var byTag = await Client.GetFromJsonAsync<List<RecipeSummaryDto>>("/api/recipes?tag=breakfast", Json);
        var bySearch = await Client.GetFromJsonAsync<List<RecipeSummaryDto>>("/api/recipes?search=curry", Json);
        var all = await Client.GetFromJsonAsync<List<RecipeSummaryDto>>("/api/recipes", Json);

        Assert.Equal([pancakesId], byTag!.Select(r => r.Id));
        Assert.Equal([curryId], bySearch!.Select(r => r.Id)); // search is case-insensitive
        Assert.Equal([curryId, pancakesId], all!.Select(r => r.Id)); // newest first
    }

    [Fact]
    public async Task Updating_a_recipe_replaces_its_ingredients_and_tags()
    {
        // Regression test: the Express version only updated the recipe row and silently dropped these.
        await LogInAsNewUserAsync();
        var id = await CreateRecipeAsync(PancakeRequest());

        var update = new
        {
            title = "Test Pancakes v2",
            instructions = "mix, cook, flip, serve",
            servings = 4,
            ingredients = new[] { new { name = "oat flour", amount = 150, unit = "g" } },
            tags = new[] { "breakfast", "gluten-free" },
        };
        var response = await Client.PutAsJsonAsync($"/api/recipes/{id}", update, Json);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var recipe = await GetRecipeAsync(id);
        Assert.Equal("Test Pancakes v2", recipe.Title);
        Assert.Equal("oat flour", Assert.Single(recipe.Ingredients).Name);
        Assert.Equal(["breakfast", "gluten-free"], recipe.Tags);
    }

    [Fact]
    public async Task Users_cannot_change_or_delete_someone_elses_recipe()
    {
        await LogInAsNewUserAsync();
        var id = await CreateRecipeAsync(PancakeRequest());

        await LogInAsNewUserAsync(); // a different user

        var update = await Client.PutAsJsonAsync($"/api/recipes/{id}", PancakeRequest("Hijacked"), Json);
        var delete = await Client.DeleteAsync($"/api/recipes/{id}");

        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
        Assert.Equal("Test Pancakes", (await GetRecipeAsync(id)).Title);
    }

    [Fact]
    public async Task Deleting_a_recipe_removes_it()
    {
        await LogInAsNewUserAsync();
        var id = await CreateRecipeAsync(PancakeRequest());

        var delete = await Client.DeleteAsync($"/api/recipes/{id}");
        var get = await Client.GetAsync($"/api/recipes/{id}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task Unknown_recipe_returns_404_with_an_error_message()
    {
        var response = await Client.GetAsync("/api/recipes/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Recipe not found", (await ReadJsonAsync(response)).GetProperty("error").GetString());
    }

    [Fact]
    public async Task Scaled_endpoint_recalculates_amounts_for_new_servings()
    {
        await LogInAsNewUserAsync();
        var id = await CreateRecipeAsync(PancakeRequest()); // 2 servings: 200 g flour, 2 eggs

        var scaled = await Client.GetFromJsonAsync<ScaledRecipeDto>($"/api/recipes/{id}/scaled?servings=6", Json);

        Assert.Equal([600m, 6m], scaled!.Ingredients.Select(i => i.Amount));
    }

    [Fact]
    public async Task Tags_are_listed_alphabetically_and_shared_between_recipes()
    {
        await LogInAsNewUserAsync();
        await CreateRecipeAsync(PancakeRequest("One", "quick", "dessert"));
        await CreateRecipeAsync(PancakeRequest("Two", "Quick"));

        var tags = await ReadJsonAsync(await Client.GetAsync("/api/tags"));

        Assert.Equal(["dessert", "quick"], tags.EnumerateArray().Select(t => t.GetProperty("name").GetString()));
    }

    [Fact]
    public async Task Unknown_routes_return_a_json_404()
    {
        var response = await Client.GetAsync("/api/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Not found", (await ReadJsonAsync(response)).GetProperty("error").GetString());
    }
}

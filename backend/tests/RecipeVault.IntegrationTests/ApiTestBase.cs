using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RecipeVault.Application.Auth;
using RecipeVault.Application.Recipes;

namespace RecipeVault.IntegrationTests;

/// <summary>Shared setup: a clean database before every test, plus helpers for common API calls.</summary>
[Collection(ApiCollection.Name)]
public abstract class ApiTestBase(ApiFactory factory) : IAsyncLifetime
{
    protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    protected HttpClient Client { get; } = factory.CreateClient();

    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }

    /// <summary>Signs up and logs in a fresh user, then sends their token on every request from this client.</summary>
    protected async Task<string> LogInAsNewUserAsync(string? email = null)
    {
        email ??= $"user-{Guid.NewGuid():N}@example.com";
        const string password = "correct-horse-battery";

        var signup = await Client.PostAsJsonAsync("/api/signup", new { email, password }, Json);
        Assert.Equal(HttpStatusCode.Created, signup.StatusCode);

        var login = await Client.PostAsJsonAsync("/api/login", new { email, password }, Json);
        login.EnsureSuccessStatusCode();
        var body = await login.Content.ReadFromJsonAsync<LoginResponse>(Json);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);
        return email;
    }

    protected static object PancakeRequest(string title = "Test Pancakes", params string[] tags) => new
    {
        title,
        description = "fluffy",
        instructions = "mix, cook, flip",
        prep_minutes = 5,
        cook_minutes = 10,
        servings = 2,
        ingredients = new object[]
        {
            new { name = "flour", amount = 200, unit = "g" },
            new { name = "egg", amount = 2, unit = (string?)null },
        },
        tags = tags.Length > 0 ? tags : ["breakfast", "quick"],
    };

    protected async Task<int> CreateRecipeAsync(object request)
    {
        var response = await Client.PostAsJsonAsync("/api/recipes", request, Json);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<RecipeIdDto>(Json);
        return body!.Id;
    }

    protected async Task<RecipeDetailDto> GetRecipeAsync(int id) =>
        (await Client.GetFromJsonAsync<RecipeDetailDto>($"/api/recipes/{id}", Json))!;

    protected static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();
}

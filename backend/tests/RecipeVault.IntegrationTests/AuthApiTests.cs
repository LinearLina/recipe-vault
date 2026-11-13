using System.Net;
using System.Net.Http.Json;

namespace RecipeVault.IntegrationTests;

public sealed class AuthApiTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Signup_returns_the_new_user_with_a_normalised_email()
    {
        var response = await Client.PostAsJsonAsync("/api/signup", new { email = "  Cook@Example.com ", password = "long-enough-password" }, Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await ReadJsonAsync(response);
        Assert.Equal("cook@example.com", body.GetProperty("email").GetString());
        Assert.True(body.GetProperty("id").GetInt32() > 0);
    }

    [Fact]
    public async Task Signing_up_twice_with_the_same_email_returns_409()
    {
        await LogInAsNewUserAsync("dupe@example.com");

        var response = await Client.PostAsJsonAsync("/api/signup", new { email = "DUPE@example.com", password = "another-password" }, Json);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("Email already in use", (await ReadJsonAsync(response)).GetProperty("error").GetString());
    }

    [Fact]
    public async Task Signup_rejects_short_passwords_and_missing_email()
    {
        var response = await Client.PostAsJsonAsync("/api/signup", new { password = "short" }, Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await ReadJsonAsync(response)).GetProperty("errors").EnumerateArray().Select(e => e.GetString()).ToList();
        Assert.Contains("email is required", errors);
        Assert.Contains("password must be at least 8 characters", errors);
    }

    [Fact]
    public async Task Login_with_the_wrong_password_returns_401_without_saying_which_part_was_wrong()
    {
        await Client.PostAsJsonAsync("/api/signup", new { email = "cook@example.com", password = "right-password" }, Json);

        var wrongPassword = await Client.PostAsJsonAsync("/api/login", new { email = "cook@example.com", password = "wrong-password" }, Json);
        var unknownEmail = await Client.PostAsJsonAsync("/api/login", new { email = "nobody@example.com", password = "right-password" }, Json);

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownEmail.StatusCode);
        Assert.Equal(
            (await ReadJsonAsync(wrongPassword)).GetProperty("error").GetString(),
            (await ReadJsonAsync(unknownEmail)).GetProperty("error").GetString());
    }

    [Fact]
    public async Task A_tampered_token_is_rejected()
    {
        await LogInAsNewUserAsync();
        var token = Client.DefaultRequestHeaders.Authorization!.Parameter!;
        Client.DefaultRequestHeaders.Authorization = new("Bearer", token[..^4] + "abcd");

        var response = await Client.PostAsJsonAsync("/api/recipes", PancakeRequest(), Json);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

using Microsoft.AspNetCore.Mvc;
using RecipeVault.Api.Errors;
using RecipeVault.Application.Auth;

namespace RecipeVault.Api.Controllers;

[Route("api")]
public sealed class AuthController(IAuthService auth) : ApiControllerBase
{
    [HttpPost("signup")]
    [ProducesResponseType<SignupResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiValidationErrors>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiError>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SignupResponse>> Signup(CredentialsRequest request, CancellationToken ct)
    {
        var result = await auth.SignupAsync(request, ct);
        return result.IsOk ? StatusCode(StatusCodes.Status201Created, result.Value) : Failure(result);
    }

    [HttpPost("login")]
    [ProducesResponseType<ApiValidationErrors>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiError>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(CredentialsRequest request, CancellationToken ct)
    {
        var result = await auth.LoginAsync(request, ct);
        return result.IsOk ? Ok(result.Value) : Failure(result);
    }
}

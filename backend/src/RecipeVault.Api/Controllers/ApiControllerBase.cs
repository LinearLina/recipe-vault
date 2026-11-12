using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using RecipeVault.Api.Errors;
using RecipeVault.Application.Common;

namespace RecipeVault.Api.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>The logged-in user's id. Only valid on actions marked [Authorize].</summary>
    protected int CurrentUserId
    {
        get
        {
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? throw new InvalidOperationException("No 'sub' claim on an authenticated request.");
            return int.Parse(sub, CultureInfo.InvariantCulture);
        }
    }

    /// <summary>Maps a failed <see cref="Result{T}"/> to the matching HTTP response.</summary>
    protected ActionResult Failure<T>(Result<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Status switch
        {
            ResultStatus.Invalid => BadRequest(new ApiValidationErrors(result.Errors)),
            ResultStatus.Unauthorized => Unauthorized(new ApiError(result.Errors[0])),
            ResultStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, new ApiError(result.Errors[0])),
            ResultStatus.NotFound => NotFound(new ApiError(result.Errors[0])),
            ResultStatus.Conflict => Conflict(new ApiError(result.Errors[0])),
            _ => throw new InvalidOperationException($"{result.Status} is not a failure status."),
        };
    }
}

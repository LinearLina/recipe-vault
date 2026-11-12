using Microsoft.AspNetCore.Mvc;

namespace RecipeVault.Api.Errors;

// Error bodies keep the shape the React client already understands:
//   { "error": "message" }            for single errors
//   { "errors": ["a", "b"] }           for validation errors
// See docs/adr/0004 for why this is used instead of ASP.NET's default ProblemDetails.

public sealed record ApiError(string Error);

public sealed record ApiValidationErrors(IReadOnlyList<string> Errors);

public static class ApiErrors
{
    /// <summary>Used when model binding fails (e.g. malformed JSON or a wrong type).</summary>
    public static IActionResult FromModelState(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var errors = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "request body is invalid" : e.ErrorMessage)
            .Distinct()
            .ToList();

        return new BadRequestObjectResult(new ApiValidationErrors(errors));
    }
}

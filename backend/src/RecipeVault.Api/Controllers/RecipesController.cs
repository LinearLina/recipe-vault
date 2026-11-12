using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeVault.Api.Errors;
using RecipeVault.Application.Recipes;

namespace RecipeVault.Api.Controllers;

[Route("api/recipes")]
public sealed class RecipesController(IRecipeService recipes) : ApiControllerBase
{
    /// <summary>Lists recipes, newest first. Optional title search and tag filter.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RecipeSummaryDto>>> List(
        [FromQuery] string? search,
        [FromQuery] string? tag,
        CancellationToken ct) =>
        Ok(await recipes.ListAsync(search, tag, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType<ApiError>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeDetailDto>> Get(int id, CancellationToken ct)
    {
        var result = await recipes.GetAsync(id, ct);
        return result.IsOk ? Ok(result.Value) : Failure(result);
    }

    /// <summary>Ingredient amounts recalculated for a different number of servings.</summary>
    [HttpGet("{id:int}/scaled")]
    [ProducesResponseType<ApiValidationErrors>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiError>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScaledRecipeDto>> Scale(int id, [FromQuery] int servings, CancellationToken ct)
    {
        var result = await recipes.ScaleAsync(id, servings, ct);
        return result.IsOk ? Ok(result.Value) : Failure(result);
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType<RecipeIdDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiValidationErrors>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RecipeIdDto>> Create(RecipeRequest request, CancellationToken ct)
    {
        var result = await recipes.CreateAsync(CurrentUserId, request, ct);
        return result.IsOk
            ? CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, result.Value)
            : Failure(result);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    [ProducesResponseType<ApiValidationErrors>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiError>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiError>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeIdDto>> Update(int id, RecipeRequest request, CancellationToken ct)
    {
        var result = await recipes.UpdateAsync(CurrentUserId, id, request, ct);
        return result.IsOk ? Ok(result.Value) : Failure(result);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiError>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiError>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await recipes.DeleteAsync(CurrentUserId, id, ct);
        return result.IsOk ? NoContent() : Failure(result);
    }
}

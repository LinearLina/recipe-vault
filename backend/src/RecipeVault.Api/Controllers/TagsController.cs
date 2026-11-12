using Microsoft.AspNetCore.Mvc;
using RecipeVault.Application.Tags;

namespace RecipeVault.Api.Controllers;

[Route("api/tags")]
public sealed class TagsController(ITagService tags) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TagDto>>> List(CancellationToken ct) =>
        Ok(await tags.ListAsync(ct));
}

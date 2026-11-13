using Microsoft.EntityFrameworkCore;
using RecipeVault.Application.Common;

namespace RecipeVault.Application.Tags;

public sealed record TagDto(int Id, string Name);

public interface ITagService
{
    Task<IReadOnlyList<TagDto>> ListAsync(CancellationToken ct);
}

public sealed class TagService(IAppDbContext db) : ITagService
{
    public async Task<IReadOnlyList<TagDto>> ListAsync(CancellationToken ct) =>
        await db.Tags.AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new TagDto(t.Id, t.Name))
            .ToListAsync(ct);
}

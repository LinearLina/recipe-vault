namespace RecipeVault.Domain;

public class Recipe
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;
    public const int ImageUrlMaxLength = 2048;

    private readonly List<Ingredient> _ingredients = [];
    private readonly List<Tag> _tags = [];

    private Recipe()
    {
    } // for EF Core

    public Recipe(int? userId, RecipeDetails details, DateTimeOffset now)
    {
        UserId = userId;
        CreatedAt = now;
        Update(details, now);
    }

    public int Id { get; private set; }

    /// <summary>The owner. Null for sample (seeded) recipes, which are read-only.</summary>
    public int? UserId { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Instructions { get; private set; } = string.Empty;
    public int PrepMinutes { get; private set; }
    public int CookMinutes { get; private set; }
    public int Servings { get; private set; }
    public string? ImageUrl { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyList<Ingredient> Ingredients => _ingredients;
    public IReadOnlyCollection<Tag> Tags => _tags;

    public bool IsOwnedBy(int userId) => UserId == userId;

    public void Update(RecipeDetails details, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(details);

        if (string.IsNullOrWhiteSpace(details.Title))
        {
            throw new DomainValidationException("title is required");
        }

        if (string.IsNullOrWhiteSpace(details.Instructions))
        {
            throw new DomainValidationException("instructions is required");
        }

        if (details.PrepMinutes < 0 || details.CookMinutes < 0)
        {
            throw new DomainValidationException("prep and cook minutes cannot be negative");
        }

        if (details.Servings < 1)
        {
            throw new DomainValidationException("servings must be at least 1");
        }

        Title = details.Title.Trim();
        Description = details.Description;
        Instructions = details.Instructions;
        PrepMinutes = details.PrepMinutes;
        CookMinutes = details.CookMinutes;
        Servings = details.Servings;
        ImageUrl = string.IsNullOrWhiteSpace(details.ImageUrl) ? null : details.ImageUrl.Trim();
        UpdatedAt = now;
    }

    /// <summary>Replaces the whole ingredient list, keeping the order it was given in.</summary>
    public void ReplaceIngredients(IEnumerable<IngredientLine> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        // Build the new list first so a bad line leaves the recipe unchanged.
        var replacement = lines.Select((line, position) => new Ingredient(line, position)).ToList();
        _ingredients.Clear();
        _ingredients.AddRange(replacement);
    }

    /// <summary>
    /// Makes the recipe's tags match <paramref name="tags"/> (case-insensitive by name).
    /// Only the differences are applied, so unchanged tags keep their existing links.
    /// </summary>
    public void SetTags(IEnumerable<Tag> tags)
    {
        ArgumentNullException.ThrowIfNull(tags);

        var desired = tags.DistinctBy(t => t.Name, StringComparer.OrdinalIgnoreCase).ToList();

        _tags.RemoveAll(existing => !desired.Exists(d => SameName(d, existing)));

        foreach (var tag in desired)
        {
            if (!_tags.Exists(existing => SameName(existing, tag)))
            {
                _tags.Add(tag);
            }
        }
    }

    /// <summary>Ingredient amounts scaled to a different number of servings, rounded to 2 decimal places.</summary>
    public IReadOnlyList<IngredientLine> ScaleIngredients(int servings)
    {
        if (servings < 1)
        {
            throw new DomainValidationException("servings must be at least 1");
        }

        var factor = (decimal)servings / Servings;

        return _ingredients
            .OrderBy(i => i.Position)
            .Select(i => new IngredientLine(
                i.Name,
                i.Amount.HasValue ? Math.Round(i.Amount.Value * factor, 2) : null,
                i.Unit))
            .ToList();
    }

    private static bool SameName(Tag a, Tag b) =>
        string.Equals(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
}

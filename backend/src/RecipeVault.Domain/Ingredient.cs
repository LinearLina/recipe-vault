namespace RecipeVault.Domain;

public class Ingredient
{
    public const int NameMaxLength = 200;
    public const int UnitMaxLength = 50;

    private Ingredient()
    {
    } // for EF Core

    internal Ingredient(IngredientLine line, int position)
    {
        if (string.IsNullOrWhiteSpace(line.Name))
        {
            throw new DomainValidationException("every ingredient needs a name");
        }

        if (line.Amount < 0)
        {
            throw new DomainValidationException("ingredient amounts cannot be negative");
        }

        Name = line.Name.Trim();
        Amount = line.Amount;
        Unit = string.IsNullOrWhiteSpace(line.Unit) ? null : line.Unit.Trim();
        Position = position;
    }

    public int Id { get; private set; }
    public int RecipeId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal? Amount { get; private set; }
    public string? Unit { get; private set; }

    /// <summary>Keeps ingredients in the order the user entered them.</summary>
    public int Position { get; private set; }
}

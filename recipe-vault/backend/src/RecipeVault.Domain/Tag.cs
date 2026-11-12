namespace RecipeVault.Domain;

public class Tag
{
    public const int NameMaxLength = 50;

    private Tag()
    {
    } // for EF Core

    public Tag(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainValidationException("tag names cannot be blank");
        }

        Name = name.Trim();
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
}

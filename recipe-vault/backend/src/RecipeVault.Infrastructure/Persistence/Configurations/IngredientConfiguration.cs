using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeVault.Domain;

namespace RecipeVault.Infrastructure.Persistence.Configurations;

internal sealed class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder.ToTable("Ingredients");

        builder.Property(i => i.Name).HasMaxLength(Ingredient.NameMaxLength).IsRequired();
        builder.Property(i => i.Unit).HasMaxLength(Ingredient.UnitMaxLength);
        builder.Property(i => i.Amount).HasPrecision(12, 3);

        builder.HasIndex(i => new { i.RecipeId, i.Position });
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeVault.Domain;

namespace RecipeVault.Infrastructure.Persistence.Configurations;

internal sealed class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable("Recipes");

        builder.Property(r => r.Title).HasMaxLength(Recipe.TitleMaxLength).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(Recipe.DescriptionMaxLength);
        builder.Property(r => r.Instructions).IsRequired();
        builder.Property(r => r.ImageUrl).HasMaxLength(Recipe.ImageUrlMaxLength);

        builder.HasIndex(r => r.Title);
        builder.HasIndex(r => r.CreatedAt);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Ingredients)
            .WithOne()
            .HasForeignKey(i => i.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(r => r.Ingredients).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(r => r.Tags)
            .WithMany()
            .UsingEntity(
                "RecipeTags",
                r => r.HasOne(typeof(Tag)).WithMany().HasForeignKey("TagId").OnDelete(DeleteBehavior.Cascade),
                l => l.HasOne(typeof(Recipe)).WithMany().HasForeignKey("RecipeId").OnDelete(DeleteBehavior.Cascade),
                j => j.HasKey("RecipeId", "TagId"));
        builder.Navigation(r => r.Tags).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

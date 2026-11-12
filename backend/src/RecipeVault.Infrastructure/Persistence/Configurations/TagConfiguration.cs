using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecipeVault.Domain;

namespace RecipeVault.Infrastructure.Persistence.Configurations;

internal sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");

        builder.Property(t => t.Name).HasMaxLength(Tag.NameMaxLength).IsRequired();

        // SQL Server's default collation is case-insensitive, so "Quick" and "quick" count as the same tag.
        builder.HasIndex(t => t.Name).IsUnique();
    }
}

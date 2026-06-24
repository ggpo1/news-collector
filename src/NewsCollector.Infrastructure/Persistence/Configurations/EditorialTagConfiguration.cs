using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsCollector.Domain.Entities;

namespace NewsCollector.Infrastructure.Persistence.Configurations;

public class EditorialTagConfiguration : IEntityTypeConfiguration<EditorialTag>
{
    public void Configure(EntityTypeBuilder<EditorialTag> builder)
    {
        builder.ToTable("editorial_tags");

        builder.HasKey(tag => tag.Id);

        builder.Property(tag => tag.Slug)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(tag => tag.Name)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(tag => tag.Color)
            .HasMaxLength(16);

        builder.Property(tag => tag.CreatedAt).IsRequired();

        builder.HasIndex(tag => tag.Slug).IsUnique();
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsCollector.Domain.Entities;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Infrastructure.Persistence.Configurations;

public class NamedEntityConfiguration : IEntityTypeConfiguration<NamedEntity>
{
    public void Configure(EntityTypeBuilder<NamedEntity> builder)
    {
        builder.ToTable("named_entities");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.Type)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(e => e.NormalizedKey)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .IsRequired();

        builder.HasIndex(e => e.NormalizedKey)
            .IsUnique();

        builder.HasIndex(e => e.Type);

        builder.HasIndex(e => e.Name);
    }
}

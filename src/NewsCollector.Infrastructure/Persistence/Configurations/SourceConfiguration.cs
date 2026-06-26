using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsCollector.Domain.Entities;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Infrastructure.Persistence.Configurations;

public class SourceConfiguration : IEntityTypeConfiguration<Source>
{
    public static readonly Guid RiaSourceId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    public void Configure(EntityTypeBuilder<Source> builder)
    {
        builder.ToTable("sources");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(s => s.Type)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(s => s.Url)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        builder.Property(s => s.FetchIntervalMinutes)
            .HasDefaultValue(15);

        builder.Property(s => s.Config)
            .HasColumnType("jsonb");

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .IsRequired();

        builder.Property(s => s.DefaultLanguage)
            .HasConversion<string>()
            .HasMaxLength(8);

        builder.HasIndex(s => s.Url)
            .IsUnique();

        builder.HasIndex(s => s.IsActive)
            .HasFilter("\"IsActive\" = true");

        builder.HasData(
            new Source
            {
                Id = RiaSourceId,
                Name = "РИА Новости",
                Type = SourceType.Rss,
                Url = "https://ria.ru/export/rss2/archive/index.xml",
                IsActive = true,
                FetchIntervalMinutes = 15,
                Config = """{"contentFetchEnabled":true,"contentSelector":".article__text"}""",
                DefaultLanguage = ContentLanguage.Ru,
                CreatedAt = new DateTimeOffset(2026, 6, 18, 0, 0, 0, TimeSpan.Zero),
                UpdatedAt = new DateTimeOffset(2026, 6, 18, 0, 0, 0, TimeSpan.Zero)
            });
    }
}

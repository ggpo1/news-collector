using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsCollector.Domain.Entities;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Infrastructure.Persistence.Configurations;

public class SearchDocumentConfiguration : IEntityTypeConfiguration<SearchDocument>
{
    public void Configure(EntityTypeBuilder<SearchDocument> builder)
    {
        builder.ToTable("search_documents");

        builder.HasKey(document => document.Id);

        builder.Property(document => document.DocumentType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(document => document.EntityId)
            .IsRequired();

        builder.Property(document => document.Language)
            .HasConversion<string>()
            .HasMaxLength(8)
            .IsRequired();

        builder.Property(document => document.Title)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(document => document.Body)
            .HasColumnType("text");

        builder.Property(document => document.SourceName)
            .HasMaxLength(256);

        builder.Property(document => document.UpdatedAt)
            .IsRequired();

        builder.HasIndex(document => new { document.DocumentType, document.EntityId })
            .IsUnique();

        builder.HasIndex(document => document.PublishedAt);
    }
}

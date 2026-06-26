using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsCollector.Domain.Entities;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Infrastructure.Persistence.Configurations;

public class NewsItemConfiguration : IEntityTypeConfiguration<NewsItem>
{
    public void Configure(EntityTypeBuilder<NewsItem> builder)
    {
        builder.ToTable("news");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.ExternalId)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(n => n.Title)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(n => n.Summary)
            .HasColumnType("text");

        builder.Property(n => n.Content)
            .HasColumnType("text");

        builder.Property(n => n.Url)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(n => n.ContentHash)
            .HasMaxLength(64);

        builder.Property(n => n.RawPayload)
            .HasColumnType("jsonb");

        builder.Property(n => n.FetchedAt)
            .IsRequired();

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.Property(n => n.ToneCoefficient)
            .HasPrecision(4, 3);

        builder.HasIndex(n => n.FetchedAt)
            .HasDatabaseName("IX_news_FetchedAt_TonePending")
            .HasFilter("\"ToneCoefficient\" IS NULL");

        builder.HasIndex(n => new { n.SourceId, n.ExternalId })
            .IsUnique();

        builder.HasIndex(n => new { n.SourceId, n.Url })
            .IsUnique();

        builder.HasIndex(n => new { n.SourceId, n.PublishedAt });

        builder.HasIndex(n => n.FetchedAt);

        builder.HasIndex(n => n.CategoryId);

        builder.Property(n => n.IsCategoryManual)
            .HasDefaultValue(false);

        builder.Property(n => n.Language)
            .HasConversion<string>()
            .HasMaxLength(8);

        builder.Property(n => n.SearchIndexedAt);

        builder.HasIndex(n => n.SearchIndexedAt)
            .HasDatabaseName("IX_news_SearchIndexedAt_Pending")
            .HasFilter("\"SearchIndexedAt\" IS NULL");

        builder.HasIndex(n => n.FetchedAt)
            .HasDatabaseName("IX_news_FetchedAt_CategoryPending")
            .HasFilter("\"CategoryId\" IS NULL AND \"IsCategoryManual\" = false");

        builder.HasIndex(n => n.FetchedAt)
            .HasDatabaseName("IX_news_FetchedAt_EntitiesPending")
            .HasFilter("\"EntitiesExtractedAt\" IS NULL");

        builder.HasIndex(n => n.ContentFetchedAt)
            .HasFilter("\"Content\" IS NULL");

        builder.HasOne(n => n.Source)
            .WithMany(s => s.NewsItems)
            .HasForeignKey(n => n.SourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.Category)
            .WithMany(c => c.NewsItems)
            .HasForeignKey(n => n.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsCollector.Domain.Entities;

namespace NewsCollector.Infrastructure.Persistence.Configurations;

public class NewsRewriteConfiguration : IEntityTypeConfiguration<NewsRewrite>
{
    public void Configure(EntityTypeBuilder<NewsRewrite> builder)
    {
        builder.ToTable("news_rewrites");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(r => r.Summary)
            .HasColumnType("text");

        builder.Property(r => r.Content)
            .HasColumnType("text");

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired();

        builder.HasIndex(r => r.SourceNewsId);

        builder.Property(r => r.AuthorId)
            .IsRequired(false);

        builder.HasIndex(r => r.AuthorId);

        builder.HasIndex(r => new { r.SourceNewsId, r.AuthorId })
            .IsUnique();

        builder.HasIndex(r => r.CreatedAt);

        builder.HasOne(r => r.SourceNews)
            .WithMany(n => n.Rewrites)
            .HasForeignKey(r => r.SourceNewsId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Author)
            .WithMany(u => u.Rewrites)
            .HasForeignKey(r => r.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

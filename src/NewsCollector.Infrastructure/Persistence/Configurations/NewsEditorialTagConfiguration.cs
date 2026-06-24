using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsCollector.Domain.Entities;

namespace NewsCollector.Infrastructure.Persistence.Configurations;

public class NewsEditorialTagConfiguration : IEntityTypeConfiguration<NewsEditorialTag>
{
    public void Configure(EntityTypeBuilder<NewsEditorialTag> builder)
    {
        builder.ToTable("news_editorial_tags");

        builder.HasKey(item => new { item.NewsItemId, item.EditorialTagId });

        builder.Property(item => item.CreatedAt).IsRequired();

        builder.HasIndex(item => item.EditorialTagId);

        builder.HasOne(item => item.NewsItem)
            .WithMany(news => news.EditorialTags)
            .HasForeignKey(item => item.NewsItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.EditorialTag)
            .WithMany(tag => tag.NewsItems)
            .HasForeignKey(item => item.EditorialTagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.AddedBy)
            .WithMany()
            .HasForeignKey(item => item.AddedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

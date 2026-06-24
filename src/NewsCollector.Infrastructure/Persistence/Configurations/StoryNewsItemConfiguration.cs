using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsCollector.Domain.Entities;

namespace NewsCollector.Infrastructure.Persistence.Configurations;

public class StoryNewsItemConfiguration : IEntityTypeConfiguration<StoryNewsItem>
{
    public void Configure(EntityTypeBuilder<StoryNewsItem> builder)
    {
        builder.ToTable("story_news_items");

        builder.HasKey(item => new { item.StoryId, item.NewsItemId });

        builder.Property(item => item.LinkedAt).IsRequired();

        builder.HasIndex(item => item.NewsItemId);

        builder.HasOne(item => item.Story)
            .WithMany(story => story.NewsItems)
            .HasForeignKey(item => item.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.NewsItem)
            .WithMany(news => news.Stories)
            .HasForeignKey(item => item.NewsItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsCollector.Domain.Entities;

namespace NewsCollector.Infrastructure.Persistence.Configurations;

public class NewsEntityMentionConfiguration : IEntityTypeConfiguration<NewsEntityMention>
{
    public void Configure(EntityTypeBuilder<NewsEntityMention> builder)
    {
        builder.ToTable("news_entity_mentions");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.MentionText)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.HasIndex(m => new { m.NewsItemId, m.NamedEntityId })
            .IsUnique();

        builder.HasIndex(m => m.NamedEntityId);

        builder.HasIndex(m => m.NewsItemId);

        builder.HasOne(m => m.NewsItem)
            .WithMany(n => n.EntityMentions)
            .HasForeignKey(m => m.NewsItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.NamedEntity)
            .WithMany(e => e.Mentions)
            .HasForeignKey(m => m.NamedEntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsCollector.Domain.Entities;

namespace NewsCollector.Infrastructure.Persistence.Configurations;

public class NewsLinkConfiguration : IEntityTypeConfiguration<NewsLink>
{
    public void Configure(EntityTypeBuilder<NewsLink> builder)
    {
        builder.ToTable("news_links", t =>
        {
            t.HasCheckConstraint("CK_news_links_ordered_ids", "\"NewsIdLow\" < \"NewsIdHigh\"");
        });

        builder.HasKey(l => l.Id);

        builder.Property(l => l.LinkType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(l => l.LinkMethod)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(l => l.Confidence)
            .HasPrecision(5, 4);

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        builder.HasIndex(l => new { l.NewsIdLow, l.NewsIdHigh })
            .IsUnique();

        builder.HasIndex(l => l.NewsIdLow);

        builder.HasIndex(l => l.NewsIdHigh);

        builder.HasOne(l => l.NewsLow)
            .WithMany(n => n.LinksAsLow)
            .HasForeignKey(l => l.NewsIdLow)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.NewsHigh)
            .WithMany(n => n.LinksAsHigh)
            .HasForeignKey(l => l.NewsIdHigh)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

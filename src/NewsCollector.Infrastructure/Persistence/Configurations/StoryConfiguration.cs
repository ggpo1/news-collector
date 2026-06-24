using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsCollector.Domain.Entities;

namespace NewsCollector.Infrastructure.Persistence.Configurations;

public class StoryConfiguration : IEntityTypeConfiguration<Story>
{
    public void Configure(EntityTypeBuilder<Story> builder)
    {
        builder.ToTable("stories");

        builder.HasKey(story => story.Id);

        builder.Property(story => story.ClusterKey)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(story => story.Title)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(story => story.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(story => story.CreatedAt).IsRequired();
        builder.Property(story => story.UpdatedAt).IsRequired();

        builder.HasIndex(story => story.ClusterKey).IsUnique();
        builder.HasIndex(story => story.Status);
        builder.HasIndex(story => story.LastActivityAt);

        builder.HasOne(story => story.PrimaryNewsItem)
            .WithMany()
            .HasForeignKey(story => story.PrimaryNewsItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(story => story.StatusUpdatedBy)
            .WithMany()
            .HasForeignKey(story => story.StatusUpdatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

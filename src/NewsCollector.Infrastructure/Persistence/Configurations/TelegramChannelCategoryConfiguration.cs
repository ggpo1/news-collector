using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsCollector.Domain.Entities;

namespace NewsCollector.Infrastructure.Persistence.Configurations;

public class TelegramChannelCategoryConfiguration : IEntityTypeConfiguration<TelegramChannelCategory>
{
    public void Configure(EntityTypeBuilder<TelegramChannelCategory> builder)
    {
        builder.ToTable("telegram_channel_categories");

        builder.HasKey(item => new { item.TelegramChannelId, item.CategoryId });

        builder.HasOne(item => item.Channel)
            .WithMany(channel => channel.ChannelCategories)
            .HasForeignKey(item => item.TelegramChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.Category)
            .WithMany()
            .HasForeignKey(item => item.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

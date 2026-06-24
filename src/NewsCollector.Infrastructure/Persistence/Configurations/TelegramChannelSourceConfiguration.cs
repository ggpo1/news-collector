using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsCollector.Domain.Entities;

namespace NewsCollector.Infrastructure.Persistence.Configurations;

public class TelegramChannelSourceConfiguration : IEntityTypeConfiguration<TelegramChannelSource>
{
    public void Configure(EntityTypeBuilder<TelegramChannelSource> builder)
    {
        builder.ToTable("telegram_channel_sources");

        builder.HasKey(item => new { item.TelegramChannelId, item.SourceId });

        builder.HasOne(item => item.Channel)
            .WithMany(channel => channel.ChannelSources)
            .HasForeignKey(item => item.TelegramChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.Source)
            .WithMany()
            .HasForeignKey(item => item.SourceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

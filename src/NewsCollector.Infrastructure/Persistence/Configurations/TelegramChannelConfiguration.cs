using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsCollector.Domain.Entities;

namespace NewsCollector.Infrastructure.Persistence.Configurations;

public class TelegramChannelConfiguration : IEntityTypeConfiguration<TelegramChannel>
{
    public void Configure(EntityTypeBuilder<TelegramChannel> builder)
    {
        builder.ToTable("telegram_channels");

        builder.HasKey(channel => channel.Id);

        builder.Property(channel => channel.Name).HasMaxLength(200).IsRequired();
        builder.Property(channel => channel.ChatId).HasMaxLength(128).IsRequired();

        builder.HasIndex(channel => new { channel.TelegramBotId, channel.ChatId }).IsUnique();

        builder.HasOne(channel => channel.Bot)
            .WithMany(bot => bot.Channels)
            .HasForeignKey(channel => channel.TelegramBotId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

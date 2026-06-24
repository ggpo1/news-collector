using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsCollector.Domain.Entities;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Infrastructure.Persistence.Configurations;

public class TelegramBotConfiguration : IEntityTypeConfiguration<TelegramBot>
{
    public void Configure(EntityTypeBuilder<TelegramBot> builder)
    {
        builder.ToTable("telegram_bots");

        builder.HasKey(bot => bot.Id);

        builder.Property(bot => bot.Name).HasMaxLength(200).IsRequired();
        builder.Property(bot => bot.BotToken).HasMaxLength(256).IsRequired();
        builder.Property(bot => bot.ContainerName).HasMaxLength(128);
        builder.Property(bot => bot.ContainerError).HasMaxLength(2000);
        builder.Property(bot => bot.ContainerStatus)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasIndex(bot => bot.Name);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsCollector.Domain.Entities;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Infrastructure.Persistence.Configurations;

public class TelegramDeliveryConfiguration : IEntityTypeConfiguration<TelegramDelivery>
{
    public void Configure(EntityTypeBuilder<TelegramDelivery> builder)
    {
        builder.ToTable("telegram_deliveries");

        builder.HasKey(delivery => delivery.Id);

        builder.Property(delivery => delivery.MessageText).IsRequired();
        builder.Property(delivery => delivery.ErrorMessage).HasMaxLength(2000);
        builder.Property(delivery => delivery.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasIndex(delivery => new { delivery.Status, delivery.CreatedAt });

        builder.HasOne(delivery => delivery.Channel)
            .WithMany()
            .HasForeignKey(delivery => delivery.TelegramChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(delivery => delivery.NewsItem)
            .WithMany()
            .HasForeignKey(delivery => delivery.NewsItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(delivery => delivery.NewsRewrite)
            .WithMany()
            .HasForeignKey(delivery => delivery.NewsRewriteId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

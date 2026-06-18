using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsCollector.Domain.Entities;

namespace NewsCollector.Infrastructure.Persistence.Configurations;

public class ApiVisitConfiguration : IEntityTypeConfiguration<ApiVisit>
{
    public void Configure(EntityTypeBuilder<ApiVisit> builder)
    {
        builder.ToTable("api_visits");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.HttpMethod)
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(v => v.Path)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(v => v.QueryString)
            .HasMaxLength(2048);

        builder.Property(v => v.VisitorFingerprint)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(v => v.UserAgent)
            .HasMaxLength(512);

        builder.Property(v => v.RequestedAt)
            .IsRequired();

        builder.HasIndex(v => v.RequestedAt);

        builder.HasIndex(v => new { v.VisitorFingerprint, v.RequestedAt });

        builder.HasIndex(v => new { v.Path, v.RequestedAt });
    }
}

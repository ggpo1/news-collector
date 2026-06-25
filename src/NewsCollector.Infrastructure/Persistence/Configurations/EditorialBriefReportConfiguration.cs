using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsCollector.Domain.Entities;
using NewsCollector.Domain.Enums;

namespace NewsCollector.Infrastructure.Persistence.Configurations;

public class EditorialBriefReportConfiguration : IEntityTypeConfiguration<EditorialBriefReport>
{
    public void Configure(EntityTypeBuilder<EditorialBriefReport> builder)
    {
        builder.ToTable("editorial_brief_reports");

        builder.HasKey(report => report.Id);

        builder.Property(report => report.Period)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(report => report.Markdown)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(report => report.GeneratedAt)
            .IsRequired();

        builder.Property(report => report.WindowStart)
            .IsRequired();

        builder.Property(report => report.WindowEnd)
            .IsRequired();

        builder.Property(report => report.Model)
            .HasMaxLength(128);

        builder.HasIndex(report => report.GeneratedAt);
        builder.HasIndex(report => new { report.Period, report.GeneratedAt });
    }
}

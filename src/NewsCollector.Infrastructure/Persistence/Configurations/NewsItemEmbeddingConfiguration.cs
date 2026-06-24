using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsCollector.Domain.Entities;

namespace NewsCollector.Infrastructure.Persistence.Configurations;

public class NewsItemEmbeddingConfiguration : IEntityTypeConfiguration<NewsItemEmbedding>
{
    public void Configure(EntityTypeBuilder<NewsItemEmbedding> builder)
    {
        builder.ToTable("news_item_embeddings");

        builder.HasKey(item => item.NewsItemId);

        builder.Property(item => item.Model)
            .HasMaxLength(128)
            .IsRequired();

        var vectorProperty = builder.Property(item => item.Vector)
            .HasColumnType("jsonb")
            .HasConversion(
                vector => JsonSerializer.Serialize(vector, (JsonSerializerOptions?)null),
                json => DeserializeVector(json));

        vectorProperty.Metadata.SetValueComparer(new ValueComparer<float[]>(
            (left, right) => left != null && right != null && left.SequenceEqual(right),
            vector => vector.Aggregate(0, (hash, value) => HashCode.Combine(hash, value.GetHashCode())),
            vector => vector.ToArray()));

        vectorProperty.IsRequired();

        builder.Property(item => item.CreatedAt)
            .IsRequired();

        builder.HasIndex(item => item.Model);

        builder.HasOne(item => item.NewsItem)
            .WithOne()
            .HasForeignKey<NewsItemEmbedding>(item => item.NewsItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static float[] DeserializeVector(string json) =>
        JsonSerializer.Deserialize<float[]>(json, (JsonSerializerOptions?)null) ?? Array.Empty<float>();
}

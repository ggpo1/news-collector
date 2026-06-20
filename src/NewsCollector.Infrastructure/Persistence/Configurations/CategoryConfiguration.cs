using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsCollector.Domain.Entities;

namespace NewsCollector.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public static readonly Guid PoliticsId = Guid.Parse("c1000001-0000-4000-8000-000000000001");
    public static readonly Guid EconomyId = Guid.Parse("c1000001-0000-4000-8000-000000000002");
    public static readonly Guid SocietyId = Guid.Parse("c1000001-0000-4000-8000-000000000003");
    public static readonly Guid WorldId = Guid.Parse("c1000001-0000-4000-8000-000000000004");
    public static readonly Guid TechId = Guid.Parse("c1000001-0000-4000-8000-000000000005");
    public static readonly Guid ScienceId = Guid.Parse("c1000001-0000-4000-8000-000000000006");
    public static readonly Guid SportsId = Guid.Parse("c1000001-0000-4000-8000-000000000007");
    public static readonly Guid CultureId = Guid.Parse("c1000001-0000-4000-8000-000000000008");
    public static readonly Guid IncidentsId = Guid.Parse("c1000001-0000-4000-8000-000000000009");
    public static readonly Guid OtherId = Guid.Parse("c1000001-0000-4000-8000-00000000000a");

    private static readonly DateTimeOffset SeedCreatedAt =
        new(2026, 6, 20, 0, 0, 0, TimeSpan.Zero);

    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Slug)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(c => c.Name)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(512);

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.HasIndex(c => c.Slug)
            .IsUnique();

        builder.HasIndex(c => c.IsActive)
            .HasFilter("\"IsActive\" = true");

        builder.HasData(
            Seed(OtherId, "other", "Другое", "Прочие новости, не подходящие под другие категории", 100),
            Seed(PoliticsId, "politics", "Политика", "Государство, выборы, законы, политические заявления", 10),
            Seed(EconomyId, "economy", "Экономика", "Финансы, бизнес, рынки, налоги, бюджет", 20),
            Seed(SocietyId, "society", "Общество", "Социальные темы, образование, здравоохранение, демография", 30),
            Seed(WorldId, "world", "В мире", "Международные события и внешняя политика", 40),
            Seed(TechId, "tech", "Технологии", "IT, интернет, гаджеты, цифровизация", 50),
            Seed(ScienceId, "science", "Наука", "Научные открытия, исследования, космос", 60),
            Seed(SportsId, "sports", "Спорт", "Спортивные события, соревнования, трансферы", 70),
            Seed(CultureId, "culture", "Культура", "Искусство, кино, музыка, литература", 80),
            Seed(IncidentsId, "incidents", "Происшествия", "Аварии, преступления, ЧП, катастрофы", 90));
    }

    private static Category Seed(Guid id, string slug, string name, string description, int sortOrder) =>
        new()
        {
            Id = id,
            Slug = slug,
            Name = name,
            Description = description,
            IsActive = true,
            SortOrder = sortOrder,
            CreatedAt = SeedCreatedAt
        };
}

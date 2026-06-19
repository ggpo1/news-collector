using Microsoft.EntityFrameworkCore;
using NewsCollector.Domain.Entities;

namespace NewsCollector.Infrastructure.Persistence;

public class NewsCollectorDbContext : DbContext
{
    public NewsCollectorDbContext(DbContextOptions<NewsCollectorDbContext> options)
        : base(options)
    {
    }

    public DbSet<Source> Sources => Set<Source>();

    public DbSet<NewsItem> NewsItems => Set<NewsItem>();

    public DbSet<NewsLink> NewsLinks => Set<NewsLink>();

    public DbSet<ApiVisit> ApiVisits => Set<ApiVisit>();

    public DbSet<NewsRewrite> NewsRewrites => Set<NewsRewrite>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NewsCollectorDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

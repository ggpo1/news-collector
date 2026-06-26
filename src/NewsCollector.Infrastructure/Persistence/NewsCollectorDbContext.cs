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

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<NewsLink> NewsLinks => Set<NewsLink>();

    public DbSet<ApiVisit> ApiVisits => Set<ApiVisit>();

    public DbSet<NewsRewrite> NewsRewrites => Set<NewsRewrite>();

    public DbSet<NamedEntity> NamedEntities => Set<NamedEntity>();

    public DbSet<NewsEntityMention> NewsEntityMentions => Set<NewsEntityMention>();

    public DbSet<NewsItemEmbedding> NewsItemEmbeddings => Set<NewsItemEmbedding>();

    public DbSet<Story> Stories => Set<Story>();

    public DbSet<StoryNewsItem> StoryNewsItems => Set<StoryNewsItem>();

    public DbSet<EditorialTag> EditorialTags => Set<EditorialTag>();

    public DbSet<NewsEditorialTag> NewsEditorialTags => Set<NewsEditorialTag>();

    public DbSet<User> Users => Set<User>();

    public DbSet<InvitationCode> InvitationCodes => Set<InvitationCode>();

    public DbSet<TelegramBot> TelegramBots => Set<TelegramBot>();

    public DbSet<TelegramChannel> TelegramChannels => Set<TelegramChannel>();

    public DbSet<TelegramChannelCategory> TelegramChannelCategories => Set<TelegramChannelCategory>();

    public DbSet<TelegramChannelSource> TelegramChannelSources => Set<TelegramChannelSource>();

    public DbSet<TelegramDelivery> TelegramDeliveries => Set<TelegramDelivery>();

    public DbSet<EditorialBriefReport> EditorialBriefReports => Set<EditorialBriefReport>();

    public DbSet<SearchDocument> SearchDocuments => Set<SearchDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NewsCollectorDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

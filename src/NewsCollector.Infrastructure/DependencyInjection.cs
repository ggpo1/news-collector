using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Options;
using NewsCollector.Infrastructure.Auth;
using NewsCollector.Infrastructure.Ai;
using NewsCollector.Infrastructure.Feeds;
using NewsCollector.Infrastructure.Persistence;
using NewsCollector.Infrastructure.Scraping;
using NewsCollector.Infrastructure.Services;

namespace NewsCollector.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<NewsCollectorDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ISourcesService, SourcesService>();
        services.AddScoped<INewsQueryService, NewsQueryService>();
        services.AddScoped<INewsLinkQueryService, NewsLinkQueryService>();
        services.AddScoped<INewsRewriteService, NewsRewriteService>();
        services.AddScoped<ICategoryQueryService, CategoryQueryService>();
        services.AddScoped<IApiVisitWriter, ApiVisitWriter>();

        services.AddSingleton<PasswordHasherService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IInvitationCodeService, InvitationCodeService>();
        services.AddScoped<AuthDataSeeder>();

        return services;
    }

    public static IServiceCollection AddContentEnrichment(this IServiceCollection services)
    {
        ConfigureArticleHttpClient(services.AddHttpClient("ArticleFetcher"));

        services.AddSingleton<IHtmlContentExtractor, HtmlContentExtractor>();
        services.AddScoped<IArticleContentEnrichmentService, ArticleContentEnrichmentService>();

        return services;
    }

    public static IServiceCollection AddCollectorServices(this IServiceCollection services)
    {
        services.AddHttpClient<IRssFeedReader, RssFeedReader>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<CollectorOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(options.RssFetchTimeoutSeconds);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/rss+xml, application/xml, text/xml, */*");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en;q=0.8");
        });

        services.AddContentEnrichment();
        services.AddScoped<INewsIngestionService, NewsIngestionService>();

        return services;
    }

    public static IServiceCollection AddTopicLinking(this IServiceCollection services)
    {
        services.AddScoped<ITopicLinkingService, TopicLinkingService>();
        return services;
    }

    public static IServiceCollection AddNewsCategorization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<NewsCategorizerOptions>(configuration.GetSection(NewsCategorizerOptions.SectionName));
        services.AddAiRewrite(configuration);
        services.AddScoped<INewsCategorizationService, NewsCategorizationService>();
        return services;
    }

    public static IServiceCollection AddAiRewrite(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.SectionName));

        services.AddHttpClient("Ollama", (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<OllamaOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            // Per-request timeout is enforced in OllamaAiNewsRewriteService; avoid HttpClient default 100s cap.
            client.Timeout = Timeout.InfiniteTimeSpan;
        });

        services.AddScoped<IAiNewsRewriteService, OllamaAiNewsRewriteService>();
        return services;
    }

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddPersistence(connectionString);
        services.AddCollectorServices();
        return services;
    }

    private static void ConfigureArticleHttpClient(IHttpClientBuilder builder)
    {
        builder.ConfigureHttpClient(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Accept.ParseAdd(
                "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en;q=0.8");
        });
    }
}

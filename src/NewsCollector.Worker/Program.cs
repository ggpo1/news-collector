using Microsoft.EntityFrameworkCore;
using NewsCollector.Infrastructure;
using NewsCollector.Infrastructure.Http;
using NewsCollector.Infrastructure.Persistence;
using NewsCollector.Worker;
using NewsCollector.Application.Options;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.Configure<CollectorOptions>(builder.Configuration.GetSection(CollectorOptions.SectionName));
builder.Services.AddInfrastructure(connectionString, builder.Configuration);
builder.Services.AddHostedService<CollectorWorker>();

var host = builder.Build();

var runMigrations = builder.Configuration.GetValue("RunMigrations", builder.Environment.IsDevelopment());
if (runMigrations)
{
    using var scope = host.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NewsCollectorDbContext>();
    await db.Database.MigrateWithAdvisoryLockAsync();
}

var proxyUrl = HttpClientProxyExtensions.ResolveProxyUrl(host.Services.GetRequiredService<IConfiguration>());
var startupLogger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
if (string.IsNullOrWhiteSpace(proxyUrl))
{
    startupLogger.LogWarning(
        "Outbound proxy is NOT configured. RSS/article fetch uses direct connection. " +
        "Set OUTBOUND_PROXY=http://host.docker.internal:10809 for blocked sources.");
}
else
{
    startupLogger.LogInformation("Outbound HTTP proxy configured: {ProxyUrl}", proxyUrl);
}

await host.RunAsync();

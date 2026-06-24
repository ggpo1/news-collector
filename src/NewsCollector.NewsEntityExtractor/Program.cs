using Microsoft.EntityFrameworkCore;
using NewsCollector.Application.Options;
using NewsCollector.Infrastructure;
using NewsCollector.Infrastructure.Persistence;
using NewsCollector.NewsEntityExtractor;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.Configure<NewsEntityExtractorOptions>(builder.Configuration.GetSection(NewsEntityExtractorOptions.SectionName));
builder.Services.AddPersistence(connectionString);
builder.Services.AddNewsEntityExtraction(builder.Configuration);
builder.Services.AddHostedService<NewsEntityExtractorWorker>();

var host = builder.Build();

var runMigrations = builder.Configuration.GetValue("RunMigrations", builder.Environment.IsDevelopment());
if (runMigrations)
{
    using var scope = host.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NewsCollectorDbContext>();
    await db.Database.MigrateWithAdvisoryLockAsync();
}

await host.RunAsync();

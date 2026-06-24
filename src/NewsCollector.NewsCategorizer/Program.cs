using Microsoft.EntityFrameworkCore;
using NewsCollector.Application.Options;
using NewsCollector.Infrastructure;
using NewsCollector.Infrastructure.Persistence;
using NewsCollector.NewsCategorizer;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.Configure<NewsCategorizerOptions>(builder.Configuration.GetSection(NewsCategorizerOptions.SectionName));
builder.Services.AddPersistence(connectionString);
builder.Services.AddNewsCategorization(builder.Configuration);
builder.Services.AddHostedService<NewsCategorizerWorker>();

var host = builder.Build();

var runMigrations = builder.Configuration.GetValue("RunMigrations", builder.Environment.IsDevelopment());
if (runMigrations)
{
    using var scope = host.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NewsCollectorDbContext>();
    await db.Database.MigrateWithAdvisoryLockAsync();
}

await host.RunAsync();

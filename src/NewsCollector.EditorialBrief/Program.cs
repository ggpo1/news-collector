using Microsoft.EntityFrameworkCore;
using NewsCollector.Application.Options;
using NewsCollector.EditorialBrief;
using NewsCollector.Infrastructure;
using NewsCollector.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.Configure<EditorialBriefOptions>(builder.Configuration.GetSection(EditorialBriefOptions.SectionName));
builder.Services.AddPersistence(connectionString);
builder.Services.AddAiRewrite(builder.Configuration);
builder.Services.AddEditorialBrief(builder.Configuration);
builder.Services.AddHostedService<EditorialBriefWorker>();

var host = builder.Build();

var runMigrations = builder.Configuration.GetValue("RunMigrations", false);
if (runMigrations)
{
    using var scope = host.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NewsCollectorDbContext>();
    await db.Database.MigrateWithAdvisoryLockAsync();
}

await host.RunAsync();

using Microsoft.EntityFrameworkCore;
using NewsCollector.Infrastructure;
using NewsCollector.Infrastructure.Persistence;
using NewsCollector.Worker;
using NewsCollector.Application.Options;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.Configure<CollectorOptions>(builder.Configuration.GetSection(CollectorOptions.SectionName));
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddHostedService<CollectorWorker>();

var host = builder.Build();

var runMigrations = builder.Configuration.GetValue("RunMigrations", builder.Environment.IsDevelopment());
if (runMigrations)
{
    using var scope = host.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NewsCollectorDbContext>();
    await db.Database.MigrateWithAdvisoryLockAsync();
}

await host.RunAsync();

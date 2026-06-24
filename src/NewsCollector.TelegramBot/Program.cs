using Microsoft.EntityFrameworkCore;
using NewsCollector.Application.Options;
using NewsCollector.Infrastructure;
using NewsCollector.Infrastructure.Persistence;
using NewsCollector.TelegramBot;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.Configure<TelegramBotWorkerOptions>(builder.Configuration.GetSection(TelegramBotWorkerOptions.SectionName));
builder.Services.AddPersistence(connectionString);
builder.Services.AddTelegramWorker(builder.Configuration);
builder.Services.AddHostedService<TelegramBotWorker>();

var host = builder.Build();

var runMigrations = builder.Configuration.GetValue("RunMigrations", builder.Environment.IsDevelopment());
if (runMigrations)
{
    using var scope = host.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NewsCollectorDbContext>();
    await db.Database.MigrateAsync();
}

await host.RunAsync();

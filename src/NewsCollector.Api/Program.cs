using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using NewsCollector.Api.Middleware;
using NewsCollector.Application.Options;
using NewsCollector.Infrastructure;
using NewsCollector.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.Configure<CollectorOptions>(builder.Configuration.GetSection(CollectorOptions.SectionName));
builder.Services.AddPersistence(connectionString);
builder.Services.AddContentEnrichment();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<NewsCollectorDbContext>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (corsOrigins.Length > 0)
        {
            policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
    });
});

var app = builder.Build();

var runMigrations = builder.Configuration.GetValue("RunMigrations", false);
if (runMigrations)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NewsCollectorDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseMiddleware<ApiVisitTrackingMiddleware>();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

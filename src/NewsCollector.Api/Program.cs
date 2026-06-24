using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NewsCollector.Api.Auth;
using NewsCollector.Api.Middleware;
using NewsCollector.Application.Abstractions;
using NewsCollector.Application.Options;
using NewsCollector.Infrastructure;
using NewsCollector.Infrastructure.Auth;
using NewsCollector.Infrastructure.Persistence;
using NewsCollector.Infrastructure.Telegram;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.Configure<CollectorOptions>(builder.Configuration.GetSection(CollectorOptions.SectionName));
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(AuthOptions.SectionName));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, HttpUserContext>();
builder.Services.AddPersistence(connectionString);
builder.Services.AddContentEnrichment(builder.Configuration);
builder.Services.AddAiRewrite(builder.Configuration);
builder.Services.AddTelegram(builder.Configuration);

var telegramOrchestrator = builder.Configuration.GetSection(TelegramBotOrchestratorOptions.SectionName);
if (string.IsNullOrWhiteSpace(telegramOrchestrator["WorkerConnectionString"]))
{
    builder.Configuration["TelegramBotOrchestrator:WorkerConnectionString"] = connectionString
        .Replace("Host=localhost", "Host=postgres")
        .Replace("127.0.0.1", "postgres");
}
builder.Services.AddHealthChecks()
    .AddDbContextCheck<NewsCollectorDbContext>();

var authOptions = builder.Configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>()
    ?? new AuthOptions();

if (string.IsNullOrWhiteSpace(authOptions.JwtSecret))
{
    throw new InvalidOperationException("Auth:JwtSecret must be configured.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.JwtSecret))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddRequestTimeouts(options =>
{
    options.AddPolicy("AiRewrite", TimeSpan.FromMinutes(30));
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
    await db.Database.MigrateWithAdvisoryLockAsync();
}

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<AuthDataSeeder>();
    await seeder.SeedAsync();

    var ollamaOptions = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OllamaOptions>>().Value;
    var startupLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    startupLogger.LogInformation(
        "Ollama configured: BaseUrl={BaseUrl}, Model={Model}, TimeoutSeconds={TimeoutSeconds}",
        ollamaOptions.BaseUrl,
        ollamaOptions.Model,
        ollamaOptions.TimeoutSeconds);

    if (ollamaOptions.TimeoutSeconds < 1800)
    {
        startupLogger.LogWarning(
            "Ollama TimeoutSeconds={TimeoutSeconds} is below 1800. Large CPU models (e.g. deepseek-r1:14b) often need 30+ minutes.",
            ollamaOptions.TimeoutSeconds);
    }

    var telegramProxy = app.Configuration["Telegram:HttpsProxy"]
        ?? app.Configuration["Telegram:HttpProxy"];
    if (string.IsNullOrWhiteSpace(telegramProxy))
    {
        startupLogger.LogWarning(
            "Telegram proxy is NOT configured. API cannot reach api.telegram.org without direct access. " +
            "Set TELEGRAM_PROXY=http://host.docker.internal:10809 in .env and recreate api container.");
    }
    else
    {
        startupLogger.LogInformation("Telegram HTTP proxy configured: {ProxyUrl}", telegramProxy);
    }

    var proxyDiagnostics = scope.ServiceProvider.GetRequiredService<TelegramProxyDiagnosticsService>();
    var proxyCheck = await proxyDiagnostics.RunAsync();
    if (proxyCheck.TelegramApiReachable)
    {
        startupLogger.LogInformation(
            "Telegram proxy check OK ({LatencyMs}ms, HTTP {StatusCode})",
            proxyCheck.TelegramApiLatencyMs,
            proxyCheck.TelegramHttpStatusCode);
    }
    else if (!string.IsNullOrWhiteSpace(telegramProxy))
    {
        startupLogger.LogError(
            "Telegram proxy check FAILED: {Summary}. TCP={TcpOk} ({TcpError}). HTTP={HttpError}. " +
            "On host: ss -tlnp | grep 10809 (must be 0.0.0.0:10809). " +
            "Try: sudo ufw allow from 172.16.0.0/12 to any port 10809",
            proxyCheck.Summary,
            proxyCheck.ProxyTcpReachable,
            proxyCheck.ProxyTcpError,
            proxyCheck.TelegramApiError);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseRequestTimeouts();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ApiVisitTrackingMiddleware>();
app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

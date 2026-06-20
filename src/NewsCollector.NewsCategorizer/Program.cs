using NewsCollector.Application.Options;
using NewsCollector.Infrastructure;
using NewsCollector.NewsCategorizer;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.Configure<NewsCategorizerOptions>(builder.Configuration.GetSection(NewsCategorizerOptions.SectionName));
builder.Services.AddPersistence(connectionString);
builder.Services.AddNewsCategorization(builder.Configuration);
builder.Services.AddHostedService<NewsCategorizerWorker>();

await builder.Build().RunAsync();

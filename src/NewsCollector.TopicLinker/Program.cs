using NewsCollector.Application.Options;
using NewsCollector.Infrastructure;
using NewsCollector.TopicLinker;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.Configure<TopicLinkerOptions>(builder.Configuration.GetSection(TopicLinkerOptions.SectionName));
builder.Services.AddPersistence(connectionString);
builder.Services.AddTopicLinking();
builder.Services.AddHostedService<TopicLinkerWorker>();

await builder.Build().RunAsync();

namespace NewsCollector.Application.Options;

public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; set; } = "http://localhost:11434";

    public string Model { get; set; } = "deepseek-r1:14b";

    public int TimeoutSeconds { get; set; } = 120;
}

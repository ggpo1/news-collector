namespace NewsCollector.Application.Options;

public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; set; } = "http://localhost:11434";

    public string Model { get; set; } = "qwen3.5:27b";

    public int TimeoutSeconds { get; set; } = 1800;

    /// <summary>Размер контекста (num_ctx). Меньше — меньше RAM/VRAM.</summary>
    public int NumCtx { get; set; } = 8192;

    /// <summary>Сколько держать модель в памяти после запроса (Ollama keep_alive).</summary>
    public string KeepAlive { get; set; } = "5m";

    /// <summary>Отключить режим «thinking» у qwen3 (экономит память и время).</summary>
    public bool Think { get; set; }
}

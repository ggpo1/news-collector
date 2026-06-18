namespace NewsCollector.Domain.Entities;

public class ApiVisit
{
    public Guid Id { get; set; }

    public DateTimeOffset RequestedAt { get; set; }

    public required string HttpMethod { get; set; }

    public required string Path { get; set; }

    public string? QueryString { get; set; }

    public int StatusCode { get; set; }

    public int DurationMs { get; set; }

    /// <summary>
    /// SHA-256 hash identifying the visitor (client id and/or IP + UA + language).
    /// </summary>
    public required string VisitorFingerprint { get; set; }

    public string? UserAgent { get; set; }
}

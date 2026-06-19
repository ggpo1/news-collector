namespace NewsCollector.Application.Options;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public string JwtSecret { get; set; } = string.Empty;

    public int JwtExpirationMinutes { get; set; } = 480;

    public string SeedChiefEditorLogin { get; set; } = "chief";

    public string SeedChiefEditorPassword { get; set; } = "changeme";

    public string SeedChiefEditorDisplayName { get; set; } = "Главный редактор";
}

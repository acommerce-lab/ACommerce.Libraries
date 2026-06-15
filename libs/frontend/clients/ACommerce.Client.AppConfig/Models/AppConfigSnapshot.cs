namespace ACommerce.Client.AppConfig.Models;

/// <summary>
/// لقطة AppConfig من الخادم — صورة طبق الأصل من الـ DTO الخادمي.
/// </summary>
public sealed class AppConfigSnapshot
{
    public string Version { get; set; } = string.Empty;
    public DateTime FetchedAt { get; set; }
    public string Language { get; set; } = "ar";
    public string? Platform { get; set; }
    public string? AppVersion { get; set; }
    public Dictionary<string, bool> Features { get; set; } = new();
    public Dictionary<string, string> Strings { get; set; } = new();
    public Dictionary<string, Dictionary<string, string>> Theme { get; set; } = new()
    {
        ["light"] = new(),
        ["dark"] = new()
    };
    public Dictionary<string, string> Config { get; set; } = new();
}

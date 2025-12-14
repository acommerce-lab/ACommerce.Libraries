namespace Ashare.Shared.Services.Analytics;

public class AnalyticsOptions
{
    public bool Enabled { get; set; } = true;
    public AnalyticsConfig Meta { get; set; } = new();
    public AnalyticsConfig Google { get; set; } = new();
    public AnalyticsConfig TikTok { get; set; } = new();
    public AnalyticsConfig Snapchat { get; set; } = new();
}

public class AnalyticsConfig
{
    public string? AppId { get; set; }
    public string? IosAppId { get; set; }
    public string? AndroidAppId { get; set; }
    public bool DebugMode { get; set; }
}

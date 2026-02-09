using ACommerce.Client.Core.Configuration;

namespace HamtramckHardware.Shared.Services;

/// <summary>
/// API Configuration for Hamtramck Hardware
/// Uses the shared acommerce-marketplace backend
/// </summary>
public class ApiConfiguration : ACommerce.Client.Core.Configuration.ApiConfiguration
{
    /// <summary>
    /// Production API URL (Google Cloud Run - US Central)
    /// </summary>
    public const string DefaultProductionUrl = "https://acommerce-marketplace-130415035604.me-central1.run.app";

    /// <summary>
    /// Development URL for Android Emulator (Ashare.Api on port 3000)
    /// </summary>
    public const string DefaultAndroidEmulatorUrl = "http://10.0.2.2:3000";

    /// <summary>
    /// Development URL for localhost (Ashare.Api on port 3000)
    /// </summary>
    public const string DefaultLocalhostUrl = "http://localhost:3000";

    /// <summary>
    /// Hamtramck Hardware API options
    /// </summary>
    public static ApiConfigurationOptions HamtramckOptions => new()
    {
        ProductionUrl = DefaultProductionUrl,
        AndroidEmulatorUrl = DefaultAndroidEmulatorUrl,
        LocalhostUrl = DefaultLocalhostUrl
    };

    public ApiConfiguration(AppPlatform platform, bool useLocalApi = false, string? customBaseUrl = null)
        : base(platform, useLocalApi, HamtramckOptions, customBaseUrl)
    {
    }

    // Factory Methods
    public new static ApiConfiguration ForWeb(bool useLocalApi = false, string? customBaseUrl = null)
        => new(AppPlatform.Web, useLocalApi, customBaseUrl);

    public new static ApiConfiguration ForAndroid(bool useLocalApi = false, string? customBaseUrl = null)
        => new(AppPlatform.Android, useLocalApi, customBaseUrl);

    public new static ApiConfiguration ForIOS(bool useLocalApi = false, string? customBaseUrl = null)
        => new(AppPlatform.iOS, useLocalApi, customBaseUrl);

    public new static ApiConfiguration ForWindows(bool useLocalApi = false, string? customBaseUrl = null)
        => new(AppPlatform.Windows, useLocalApi, customBaseUrl);
}

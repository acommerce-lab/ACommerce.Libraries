namespace Ashare.App.Services;

/// <summary>
/// Centralized API configuration for Ashare App.
/// All API base URLs should be retrieved from this single source.
/// </summary>
public class ApiSettings
{
    /// <summary>
    /// Production API URL
    /// </summary>
    public const string ProductionUrl = "https://api.ashare.app";

    /// <summary>
    /// Development URL for Android Emulator (10.0.2.2 maps to host's localhost)
    /// </summary>
    public const string AndroidEmulatorUrl = "https://10.0.2.2:5001";

    /// <summary>
    /// Development URL for Windows/Desktop
    /// </summary>
    public const string LocalhostUrl = "https://localhost:5001";

    /// <summary>
    /// Gets the appropriate API base URL based on the current platform and build configuration.
    /// </summary>
    public static string BaseUrl
    {
        get
        {
#if DEBUG
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                return AndroidEmulatorUrl;
            }
            else if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                return LocalhostUrl;
            }
            // iOS Simulator and other platforms in debug mode
            return LocalhostUrl;
#else
            return ProductionUrl;
#endif
        }
    }

    /// <summary>
    /// Gets the base URL as a Uri object.
    /// </summary>
    public static Uri BaseUri => new Uri(BaseUrl);
}

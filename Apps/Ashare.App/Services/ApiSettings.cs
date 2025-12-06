namespace Ashare.App.Services;

/// <summary>
/// Centralized API configuration for Ashare App.
/// All API base URLs should be retrieved from this single source.
/// </summary>
public static class ApiSettings
{
    // ═══════════════════════════════════════════════════════════════════════════
    // 🔧 التبديل السريع بين البيئات
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 🔄 تبديل سريع للبيئة:
    /// - true = استخدام الباك اند المحلي (localhost)
    /// - false = استخدام الباك اند الإنتاجي (Azure)
    /// </summary>
    public const bool UseLocalApi = true;  // ← غيّر هذا للتبديل

    // ═══════════════════════════════════════════════════════════════════════════
    // 📍 عناوين الـ API
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Production API URL
    /// </summary>
    public const string ProductionUrl = "https://ashareapi-hygabpf3ajfmevfs.canadaeast-01.azurewebsites.net";

    /// <summary>
    /// Development URL for Android Emulator (10.0.2.2 maps to host's localhost)
    /// </summary>
    public const string AndroidEmulatorUrl = "https://10.0.2.2:5001";

    /// <summary>
    /// Development URL for iOS Simulator
    /// </summary>
    public const string IosSimulatorUrl = "https://localhost:5001";

    /// <summary>
    /// Development URL for Windows/Desktop
    /// </summary>
    public const string LocalhostUrl = "https://localhost:5001";

    // ═══════════════════════════════════════════════════════════════════════════
    // 🎯 الـ URL المستخدم
    // ═══════════════════════════════════════════════════════════════════════════

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

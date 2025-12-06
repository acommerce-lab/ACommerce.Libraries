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
    /// Production API URL (Azure)
    /// </summary>
    public const string ProductionUrl = "https://ashareapi-hygabpf3ajfmevfs.canadaeast-01.azurewebsites.net";

    /// <summary>
    /// Development URL for Android Emulator (10.0.2.2 maps to host's localhost)
    /// </summary>
    public const string AndroidEmulatorUrl = "https://10.0.2.2:5001";

    /// <summary>
    /// Development URL for Windows/Desktop
    /// </summary>
    public const string LocalhostUrl = "https://localhost:5001";

    // ═══════════════════════════════════════════════════════════════════════════
    // 🎯 الـ URL المستخدم
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the appropriate API base URL based on UseLocalApi setting and platform.
    /// </summary>
    public static string BaseUrl
    {
        get
        {
            // إذا تم اختيار الباك اند المحلي
            if (UseLocalApi)
            {
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    return AndroidEmulatorUrl;
                }
                else if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    return LocalhostUrl;
                }
                return LocalhostUrl;
            }

            // استخدام الباك اند الإنتاجي
            return ProductionUrl;
        }
    }

    /// <summary>
    /// Gets the base URL as a Uri object.
    /// </summary>
    public static Uri BaseUri => new Uri(BaseUrl);

    /// <summary>
    /// Logs current configuration (for debugging)
    /// </summary>
    public static void LogConfiguration()
    {
        System.Diagnostics.Debug.WriteLine($"[ApiSettings] UseLocalApi: {UseLocalApi}");
        System.Diagnostics.Debug.WriteLine($"[ApiSettings] Platform: {DeviceInfo.Platform}");
        System.Diagnostics.Debug.WriteLine($"[ApiSettings] BaseUrl: {BaseUrl}");
    }
}
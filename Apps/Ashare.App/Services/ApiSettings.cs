namespace Ashare.App.Services;

/// <summary>
/// Centralized API configuration for Ashare App.
/// All API base URLs should be retrieved from this single source.
/// </summary>
public class ApiSettings
{
    // ═══════════════════════════════════════════════════════════════════
    // ⚙️ التبديل الرئيسي - غير هذه القيمة فقط للتبديل بين البيئات
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// true = استخدام الباك اند المحلي (localhost)
    /// false = استخدام الباك اند العالمي (Azure)
    /// </summary>
    public const bool UseLocalApi = false;

    /// <summary>
    /// رقم الهوية المقبول للاختبار (يعمل فقط مع الباك اند المحلي)
    /// </summary>
    public const string TestNationalId = "2507643761";

    // ═══════════════════════════════════════════════════════════════════
    // روابط الـ API
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Production API URL (Azure Canada East)
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

    /// <summary>
    /// Gets the appropriate API base URL based on configuration.
    /// </summary>
    public static string BaseUrl
    {
        get
        {
            // إذا كان UseLocalApi = false، استخدم Production دائماً
            if (!UseLocalApi)
            {
                return ProductionUrl;
            }

            // UseLocalApi = true، اختر الرابط المناسب للمنصة
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                return AndroidEmulatorUrl;
            }
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                return IosSimulatorUrl;
            }

            return LocalhostUrl;
        }
    }

    /// <summary>
    /// Gets the base URL as a Uri object.
    /// </summary>
    public static Uri BaseUri => new Uri(BaseUrl);

    /// <summary>
    /// هل نحن في وضع التطوير المحلي؟
    /// </summary>
    public static bool IsLocalDevelopment => UseLocalApi;
}

namespace Ashare.Shared.Services;

/// <summary>
/// إعدادات API الموحدة - تُهيَّأ حسب المنصة عند بدء التطبيق
/// </summary>
public class ApiConfiguration : IApiConfiguration
{
    // ═══════════════════════════════════════════════════════════════════════════
    // 📍 عناوين الـ API الافتراضية
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Production API URL (Google Cloud Run - Dammam)
    /// </summary>
    public const string DefaultProductionUrl = "https://ashare-api-130415035604.me-central2.run.app";

    /// <summary>
    /// Development URL for Android Emulator (10.0.2.2 maps to host's localhost)
    /// </summary>
    public const string DefaultAndroidEmulatorUrl = "http://10.0.2.2:8080";

    /// <summary>
    /// Development URL for Windows/Desktop/Web
    /// </summary>
    public const string DefaultLocalhostUrl = "http://localhost:8080";

    // ═══════════════════════════════════════════════════════════════════════════
    // 🎯 الخصائص
    // ═══════════════════════════════════════════════════════════════════════════

    private readonly string _baseUrl;
    private readonly bool _isLocal;
    private readonly AppPlatform _platform;

    /// <summary>
    /// إنشاء إعدادات API
    /// </summary>
    /// <param name="platform">نوع المنصة</param>
    /// <param name="useLocalApi">استخدام البيئة المحلية</param>
    /// <param name="customBaseUrl">عنوان مخصص (اختياري)</param>
    public ApiConfiguration(AppPlatform platform, bool useLocalApi = false, string? customBaseUrl = null)
    {
        _platform = platform;
        _isLocal = useLocalApi;

        if (!string.IsNullOrEmpty(customBaseUrl))
        {
            _baseUrl = customBaseUrl;
        }
        else if (useLocalApi)
        {
            _baseUrl = GetLocalUrl(platform);
        }
        else
        {
            _baseUrl = DefaultProductionUrl;
        }

        LogConfiguration();
    }

    /// <summary>
    /// عنوان API الأساسي
    /// </summary>
    public string BaseUrl => _baseUrl;

    /// <summary>
    /// عنوان API كـ Uri
    /// </summary>
    public Uri BaseUri => new Uri(_baseUrl);

    /// <summary>
    /// هل نستخدم البيئة المحلية؟
    /// </summary>
    public bool IsLocalEnvironment => _isLocal;

    /// <summary>
    /// نوع المنصة الحالية
    /// </summary>
    public AppPlatform Platform => _platform;

    // ═══════════════════════════════════════════════════════════════════════════
    // 🔧 المساعدات
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// الحصول على عنوان البيئة المحلية حسب المنصة
    /// </summary>
    private static string GetLocalUrl(AppPlatform platform)
    {
        return platform switch
        {
            AppPlatform.Android => DefaultAndroidEmulatorUrl,
            _ => DefaultLocalhostUrl
        };
    }

    /// <summary>
    /// طباعة الإعدادات للتشخيص
    /// </summary>
    private void LogConfiguration()
    {
        Console.WriteLine($"[ApiConfiguration] Platform: {_platform}");
        Console.WriteLine($"[ApiConfiguration] IsLocal: {_isLocal}");
        Console.WriteLine($"[ApiConfiguration] BaseUrl: {_baseUrl}");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // 🏭 Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// إنشاء إعدادات للويب
    /// </summary>
    public static ApiConfiguration ForWeb(bool useLocalApi = false, string? customBaseUrl = null)
        => new(AppPlatform.Web, useLocalApi, customBaseUrl);

    /// <summary>
    /// إنشاء إعدادات لأندرويد
    /// </summary>
    public static ApiConfiguration ForAndroid(bool useLocalApi = false, string? customBaseUrl = null)
        => new(AppPlatform.Android, useLocalApi, customBaseUrl);

    /// <summary>
    /// إنشاء إعدادات لـ iOS
    /// </summary>
    public static ApiConfiguration ForIOS(bool useLocalApi = false, string? customBaseUrl = null)
        => new(AppPlatform.iOS, useLocalApi, customBaseUrl);

    /// <summary>
    /// إنشاء إعدادات لـ Windows
    /// </summary>
    public static ApiConfiguration ForWindows(bool useLocalApi = false, string? customBaseUrl = null)
        => new(AppPlatform.Windows, useLocalApi, customBaseUrl);
}

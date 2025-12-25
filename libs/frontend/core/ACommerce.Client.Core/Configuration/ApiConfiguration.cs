namespace ACommerce.Client.Core.Configuration;

/// <summary>
/// خيارات إعدادات API
/// </summary>
public class ApiConfigurationOptions
{
    /// <summary>
    /// عنوان البيئة الإنتاجية
    /// </summary>
    public string ProductionUrl { get; set; } = "http://localhost:8080";

    /// <summary>
    /// عنوان البيئة المحلية لمحاكي Android
    /// </summary>
    public string AndroidEmulatorUrl { get; set; } = "http://10.0.2.2:8080";

    /// <summary>
    /// عنوان البيئة المحلية (localhost)
    /// </summary>
    public string LocalhostUrl { get; set; } = "http://localhost:8080";
}

/// <summary>
/// إعدادات API الموحدة - تُهيَّأ حسب المنصة عند بدء التطبيق
/// </summary>
public class ApiConfiguration : IApiConfiguration
{
    private readonly string _baseUrl;
    private readonly bool _isLocal;
    private readonly AppPlatform _platform;

    /// <summary>
    /// إنشاء إعدادات API
    /// </summary>
    /// <param name="platform">نوع المنصة</param>
    /// <param name="useLocalApi">استخدام البيئة المحلية</param>
    /// <param name="options">خيارات العناوين</param>
    /// <param name="customBaseUrl">عنوان مخصص (اختياري)</param>
    public ApiConfiguration(
        AppPlatform platform,
        bool useLocalApi = false,
        ApiConfigurationOptions? options = null,
        string? customBaseUrl = null)
    {
        _platform = platform;
        _isLocal = useLocalApi;
        var opts = options ?? new ApiConfigurationOptions();

        if (!string.IsNullOrEmpty(customBaseUrl))
        {
            _baseUrl = customBaseUrl;
        }
        else if (useLocalApi)
        {
            _baseUrl = GetLocalUrl(platform, opts);
        }
        else
        {
            _baseUrl = opts.ProductionUrl;
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

    /// <summary>
    /// الحصول على عنوان البيئة المحلية حسب المنصة
    /// </summary>
    private static string GetLocalUrl(AppPlatform platform, ApiConfigurationOptions opts)
    {
        return platform switch
        {
            AppPlatform.Android => opts.AndroidEmulatorUrl,
            _ => opts.LocalhostUrl
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
    public static ApiConfiguration ForWeb(bool useLocalApi = false, ApiConfigurationOptions? options = null, string? customBaseUrl = null)
        => new(AppPlatform.Web, useLocalApi, options, customBaseUrl);

    /// <summary>
    /// إنشاء إعدادات لأندرويد
    /// </summary>
    public static ApiConfiguration ForAndroid(bool useLocalApi = false, ApiConfigurationOptions? options = null, string? customBaseUrl = null)
        => new(AppPlatform.Android, useLocalApi, options, customBaseUrl);

    /// <summary>
    /// إنشاء إعدادات لـ iOS
    /// </summary>
    public static ApiConfiguration ForIOS(bool useLocalApi = false, ApiConfigurationOptions? options = null, string? customBaseUrl = null)
        => new(AppPlatform.iOS, useLocalApi, options, customBaseUrl);

    /// <summary>
    /// إنشاء إعدادات لـ Windows
    /// </summary>
    public static ApiConfiguration ForWindows(bool useLocalApi = false, ApiConfigurationOptions? options = null, string? customBaseUrl = null)
        => new(AppPlatform.Windows, useLocalApi, options, customBaseUrl);
}

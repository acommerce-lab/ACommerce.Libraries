using ACommerce.Client.Core.Configuration;

namespace Nadena.Shared.Services;

/// <summary>
/// إعدادات API لتطبيق نادينا
/// </summary>
public class ApiConfiguration : ACommerce.Client.Core.Configuration.ApiConfiguration
{
    // ═══════════════════════════════════════════════════════════════════════════
    // 📍 عناوين نادينا
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Production API URL (Alibaba Cloud - Saudi Arabia)
    /// </summary>
    public const string DefaultProductionUrl = "https://api.ashare.sa";

    /// <summary>
    /// Development URL for Android Emulator (10.0.2.2 maps to host's localhost)
    /// </summary>
    public const string DefaultAndroidEmulatorUrl = "http://10.0.2.2:8080";

    /// <summary>
    /// Development URL for Windows/Desktop/Web
    /// </summary>
    public const string DefaultLocalhostUrl = "http://localhost:8080";

    /// <summary>
    /// إعدادات نادينا الافتراضية
    /// </summary>
    public static ApiConfigurationOptions NadenaOptions => new()
    {
        ProductionUrl = DefaultProductionUrl,
        AndroidEmulatorUrl = DefaultAndroidEmulatorUrl,
        LocalhostUrl = DefaultLocalhostUrl
    };

    /// <summary>
    /// إنشاء إعدادات API لنادينا
    /// </summary>
    public ApiConfiguration(AppPlatform platform, bool useLocalApi = false, string? customBaseUrl = null)
        : base(platform, useLocalApi, NadenaOptions, customBaseUrl)
    {
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // 🏭 Factory Methods for Ashare
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// إنشاء إعدادات للويب
    /// </summary>
    public new static ApiConfiguration ForWeb(bool useLocalApi = false, string? customBaseUrl = null)
        => new(AppPlatform.Web, useLocalApi, customBaseUrl);

    /// <summary>
    /// إنشاء إعدادات لأندرويد
    /// </summary>
    public new static ApiConfiguration ForAndroid(bool useLocalApi = false, string? customBaseUrl = null)
        => new(AppPlatform.Android, useLocalApi, customBaseUrl);

    /// <summary>
    /// إنشاء إعدادات لـ iOS
    /// </summary>
    public new static ApiConfiguration ForIOS(bool useLocalApi = false, string? customBaseUrl = null)
        => new(AppPlatform.iOS, useLocalApi, customBaseUrl);

    /// <summary>
    /// إنشاء إعدادات لـ Windows
    /// </summary>
    public new static ApiConfiguration ForWindows(bool useLocalApi = false, string? customBaseUrl = null)
        => new(AppPlatform.Windows, useLocalApi, customBaseUrl);
}

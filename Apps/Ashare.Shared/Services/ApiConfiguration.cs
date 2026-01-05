using ACommerce.Client.Core.Configuration;

namespace Ashare.Shared.Services;

/// <summary>
/// إعدادات API لتطبيق عشير
/// </summary>
public class ApiConfiguration : ACommerce.Client.Core.Configuration.ApiConfiguration
{
    // ═══════════════════════════════════════════════════════════════════════════
    // 📍 عناوين عشير
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

    /// <summary>
    /// إعدادات عشير الافتراضية
    /// </summary>
    public static ApiConfigurationOptions AshareOptions => new()
    {
        ProductionUrl = DefaultProductionUrl,
        AndroidEmulatorUrl = DefaultAndroidEmulatorUrl,
        LocalhostUrl = DefaultLocalhostUrl
    };

    /// <summary>
    /// إنشاء إعدادات API لعشير
    /// </summary>
    public ApiConfiguration(AppPlatform platform, bool useLocalApi = false, string? customBaseUrl = null)
        : base(platform, useLocalApi, AshareOptions, customBaseUrl)
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

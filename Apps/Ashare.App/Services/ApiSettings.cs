using ACommerce.Client.Core.Configuration;
using Ashare.Shared.Services;
using Microsoft.Maui.Devices;
using AshareApiConfig = Ashare.Shared.Services.ApiConfiguration;

namespace Ashare.App.Services;

/// <summary>
/// Centralized API configuration for Ashare App.
/// يستخدم ApiConfiguration المشترك مع دعم التوافق العكسي.
/// </summary>
public static class ApiSettings
{
    // ═══════════════════════════════════════════════════════════════════════════
    // 🔧 التبديل السريع بين البيئات
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 🔄 تبديل سريع للبيئة:
    /// - true = استخدام الباك اند المحلي (localhost)
    /// - false = استخدام الباك اند الإنتاجي (Google Cloud)
    /// </summary>
#if DEBUG
    public const bool UseLocalApi = false;  // ← للتطوير فقط
#else
    public const bool UseLocalApi = false; // ← للإنتاج
#endif

    // ═══════════════════════════════════════════════════════════════════════════
    // 📍 عناوين الـ API (للتوافق العكسي)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Production API URL (Google Cloud Run - Dammam)
    /// </summary>
    public const string ProductionUrl = AshareApiConfig.DefaultProductionUrl;

    /// <summary>
    /// Development URL for Android Emulator (10.0.2.2 maps to host's localhost)
    /// </summary>
    public const string AndroidEmulatorUrl = AshareApiConfig.DefaultAndroidEmulatorUrl;

    /// <summary>
    /// Development URL for Windows/Desktop
    /// </summary>
    public const string LocalhostUrl = AshareApiConfig.DefaultLocalhostUrl;

    // ═══════════════════════════════════════════════════════════════════════════
    // 🎯 الإعدادات المشتركة
    // ═══════════════════════════════════════════════════════════════════════════

    private static IApiConfiguration? _configuration;

    /// <summary>
    /// تهيئة الإعدادات - يُستدعى عند بدء التطبيق
    /// </summary>
    public static void Initialize()
    {
        var platform = GetCurrentPlatform();
        _configuration = new AshareApiConfig(platform, UseLocalApi);
    }

    /// <summary>
    /// الحصول على المنصة الحالية من DeviceInfo
    /// </summary>
    private static AppPlatform GetCurrentPlatform()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
            return AppPlatform.Android;
        if (DeviceInfo.Platform == DevicePlatform.iOS)
            return AppPlatform.iOS;
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
            return AppPlatform.Windows;
        if (DeviceInfo.Platform == DevicePlatform.MacCatalyst)
            return AppPlatform.MacOS;

        return AppPlatform.Unknown;
    }

    /// <summary>
    /// Gets the appropriate API base URL based on UseLocalApi setting and platform.
    /// </summary>
    public static string BaseUrl
    {
        get
        {
            // استخدام الإعدادات المشتركة إذا كانت مهيأة
            if (_configuration != null)
                return _configuration.BaseUrl;

            // Fallback للتوافق العكسي
            if (UseLocalApi)
            {
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    // ملاحظة: تم التعديل لاستخدام الإنتاج مؤقتاً
                    return AndroidEmulatorUrl;
                    //return ProductionUrl;
                }
                else if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    return LocalhostUrl;
                }
                return LocalhostUrl;
            }

            return ProductionUrl;
        }
    }

    /// <summary>
    /// Gets the base URL as a Uri object.
    /// </summary>
    public static Uri BaseUri => new Uri(BaseUrl);

    /// <summary>
    /// الحصول على الإعدادات كـ IApiConfiguration
    /// </summary>
    public static IApiConfiguration Configuration
    {
        get
        {
            if (_configuration == null)
                Initialize();
            return _configuration!;
        }
    }

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

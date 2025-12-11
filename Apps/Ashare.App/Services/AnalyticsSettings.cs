namespace Ashare.App.Services;

/// <summary>
/// إعدادات التحليلات لتتبع الحملات التسويقية
/// Analytics settings for marketing campaign tracking
///
/// ═══════════════════════════════════════════════════════════════════════════
/// كيفية الحصول على المعرفات:
/// ═══════════════════════════════════════════════════════════════════════════
///
/// 📘 Meta (Facebook/Instagram):
///    للويب (Pixel): https://business.facebook.com/events_manager → Data Sources
///    للجوال (SDK): https://developers.facebook.com → My Apps → Create App
///                  → Add iOS/Android platforms → Get App ID
///
/// 📗 Google:
///    للويب (GA4): https://analytics.google.com → Measurement ID (G-XXXXXXXXXX)
///    للجوال (Firebase): https://console.firebase.google.com → Project Settings
///                       → Add iOS/Android apps → Download config files
///
/// 📙 TikTok:
///    للويب: https://ads.tiktok.com → Assets → Events → Web Events
///    للجوال: https://ads.tiktok.com → Assets → Events → App Events → Add App
///
/// 📕 Snapchat:
///    للويب: https://ads.snapchat.com → Events Manager → Create Pixel
///    للجوال: https://ads.snapchat.com → Events Manager → Mobile App
///
/// ═══════════════════════════════════════════════════════════════════════════
/// </summary>
public static class AnalyticsSettings
{
    // ═══════════════════════════════════════════════════════════════════
    // Meta (Facebook/Instagram)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Meta Pixel ID للويب (من Events Manager)
    /// </summary>
    public const string MetaPixelId = "1775307086509416";

    /// <summary>
    /// Meta App ID للجوال - iOS (من developers.facebook.com)
    /// </summary>
    public const string MetaIosAppId = "1775307086509416";

    /// <summary>
    /// Meta App ID للجوال - Android (من developers.facebook.com)
    /// </summary>
    public const string MetaAndroidAppId = "1775307086509416";
    
    /// <summary>
    /// Meta Client Token (من Settings → Advanced → Client Token)
    /// </summary>
    public const string MetaClientToken = ""; // TODO: اطلب من مدير التسويق

    // ═══════════════════════════════════════════════════════════════════
    // Google Analytics / Firebase
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Google Analytics 4 Measurement ID للويب (G-XXXXXXXXXX)
    /// </summary>
    public const string GoogleMeasurementId = "";

    /// <summary>
    /// Firebase iOS App ID (من Project Settings)
    /// </summary>
    public const string FirebaseIosAppId = "";

    /// <summary>
    /// Firebase Android App ID (من Project Settings)
    /// </summary>
    public const string FirebaseAndroidAppId = "";

    // ═══════════════════════════════════════════════════════════════════
    // TikTok
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// TikTok Pixel ID للويب
    /// </summary>
    public const string TikTokPixelId = "";

    /// <summary>
    /// TikTok App ID للجوال - iOS
    /// </summary>
    public const string TikTokIosAppId = "";

    /// <summary>
    /// TikTok App ID للجوال - Android
    /// </summary>
    public const string TikTokAndroidAppId = "";

    // ═══════════════════════════════════════════════════════════════════
    // Snapchat
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Snapchat Pixel ID للويب
    /// </summary>
    public const string SnapchatPixelId = "";

    /// <summary>
    /// Snapchat App ID للجوال - iOS
    /// </summary>
    public const string SnapchatIosAppId = "";

    /// <summary>
    /// Snapchat App ID للجوال - Android
    /// </summary>
    public const string SnapchatAndroidAppId = "";

    // ═══════════════════════════════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// الحصول على معرف Meta المناسب للمنصة الحالية
    /// </summary>
    public static string GetMetaAppId()
    {
#if IOS || MACCATALYST
        return MetaIosAppId;
#elif ANDROID
        return MetaAndroidAppId;
#else
        return MetaPixelId; // Web/Desktop uses Pixel
#endif
    }

    /// <summary>
    /// الحصول على معرف Google/Firebase المناسب للمنصة الحالية
    /// </summary>
    public static string GetGoogleAppId()
    {
#if IOS || MACCATALYST
        return FirebaseIosAppId;
#elif ANDROID
        return FirebaseAndroidAppId;
#else
        return GoogleMeasurementId; // Web uses GA4
#endif
    }

    /// <summary>
    /// الحصول على معرف TikTok المناسب للمنصة الحالية
    /// </summary>
    public static string GetTikTokAppId()
    {
#if IOS || MACCATALYST
        return TikTokIosAppId;
#elif ANDROID
        return TikTokAndroidAppId;
#else
        return TikTokPixelId;
#endif
    }

    /// <summary>
    /// الحصول على معرف Snapchat المناسب للمنصة الحالية
    /// </summary>
    public static string GetSnapchatAppId()
    {
#if IOS || MACCATALYST
        return SnapchatIosAppId;
#elif ANDROID
        return SnapchatAndroidAppId;
#else
        return SnapchatPixelId;
#endif
    }

    /// <summary>
    /// هل التحليلات ممكّنة؟ (أي معرف واحد على الأقل موجود)
    /// </summary>
    public static bool IsEnabled =>
        !string.IsNullOrEmpty(GetMetaAppId()) ||
        !string.IsNullOrEmpty(GetGoogleAppId()) ||
        !string.IsNullOrEmpty(GetTikTokAppId()) ||
        !string.IsNullOrEmpty(GetSnapchatAppId());

    /// <summary>
    /// وضع التصحيح (يُظهر السجلات في Console)
    /// </summary>
#if DEBUG
    public const bool DebugMode = true;
#else
    public const bool DebugMode = false;
#endif
}

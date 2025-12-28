namespace Ashare.App.Services.Attribution;

/// <summary>
/// بيانات الإسناد التسويقي
/// تُلتقط من الروابط العميقة وتُرسل مع الأحداث التسويقية
/// </summary>
public class AttributionData
{
    #region Facebook / Meta

    /// <summary>
    /// Facebook Click ID - يُلتقط من fbclid في URL
    /// </summary>
    public string? Fbclid { get; set; }

    /// <summary>
    /// Facebook Browser ID - يُولّد ويُحفظ محلياً
    /// التنسيق: fb.1.{timestamp}.{random}
    /// </summary>
    public string? Fbp { get; set; }

    /// <summary>
    /// Facebook Click ID المُنسّق للـ CAPI
    /// التنسيق: fb.1.{timestamp}.{fbclid}
    /// </summary>
    public string? Fbc { get; set; }

    #endregion

    #region Google

    /// <summary>
    /// Google Click ID - يُلتقط من gclid في URL
    /// </summary>
    public string? Gclid { get; set; }

    /// <summary>
    /// Google Analytics Client ID
    /// </summary>
    public string? GaClientId { get; set; }

    #endregion

    #region TikTok

    /// <summary>
    /// TikTok Click ID - يُلتقط من ttclid في URL
    /// </summary>
    public string? Ttclid { get; set; }

    #endregion

    #region UTM Parameters

    /// <summary>
    /// مصدر الحملة (facebook, google, tiktok, etc.)
    /// </summary>
    public string? UtmSource { get; set; }

    /// <summary>
    /// وسيط الحملة (cpc, social, email, etc.)
    /// </summary>
    public string? UtmMedium { get; set; }

    /// <summary>
    /// اسم الحملة
    /// </summary>
    public string? UtmCampaign { get; set; }

    /// <summary>
    /// المحتوى (للاختبارات A/B)
    /// </summary>
    public string? UtmContent { get; set; }

    /// <summary>
    /// الكلمة المفتاحية (للإعلانات المدفوعة)
    /// </summary>
    public string? UtmTerm { get; set; }

    #endregion

    #region Metadata

    /// <summary>
    /// الرابط الأصلي الذي جاء منه المستخدم
    /// </summary>
    public string? OriginalUrl { get; set; }

    /// <summary>
    /// وقت التقاط البيانات
    /// </summary>
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// وقت انتهاء صلاحية البيانات (عادة 7 أيام)
    /// </summary>
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

    /// <summary>
    /// هل البيانات منتهية الصلاحية؟
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// هل توجد بيانات إسناد؟
    /// </summary>
    public bool HasAttribution =>
        !string.IsNullOrEmpty(Fbclid) ||
        !string.IsNullOrEmpty(Gclid) ||
        !string.IsNullOrEmpty(Ttclid) ||
        !string.IsNullOrEmpty(UtmSource);

    #endregion
}

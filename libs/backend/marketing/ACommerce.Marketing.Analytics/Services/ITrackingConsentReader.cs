namespace ACommerce.Marketing.Analytics.Services;

/// <summary>
/// خدمة قراءة حالة موافقة التتبع من الطلب
/// تقرأ الهيدر X-Tracking-Consent من الـ HTTP Request
/// </summary>
public interface ITrackingConsentReader
{
    /// <summary>
    /// هل التتبع مسموح للطلب الحالي؟
    /// يقرأ الهيدر X-Tracking-Consent ويتحقق من القيمة
    /// </summary>
    bool IsTrackingAllowed();
}

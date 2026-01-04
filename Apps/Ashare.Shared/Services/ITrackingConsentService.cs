namespace Ashare.Shared.Services;

/// <summary>
/// خدمة إدارة موافقة التتبع (App Tracking Transparency)
/// تخزن حالة الموافقة محلياً وترسلها مع كل طلب
/// </summary>
public interface ITrackingConsentService
{
    /// <summary>
    /// حالة موافقة التتبع الحالية
    /// </summary>
    TrackingConsentStatus ConsentStatus { get; }

    /// <summary>
    /// هل تم طلب الموافقة من المستخدم؟
    /// </summary>
    bool HasRequestedConsent { get; }

    /// <summary>
    /// هل التتبع مسموح؟
    /// </summary>
    bool IsTrackingAllowed { get; }

    /// <summary>
    /// طلب موافقة التتبع من المستخدم (يعرض نافذة ATT على iOS)
    /// </summary>
    Task<TrackingConsentStatus> RequestConsentAsync();

    /// <summary>
    /// الحصول على قيمة الهيدر للإرسال مع الطلبات
    /// </summary>
    string GetTrackingHeaderValue();

    /// <summary>
    /// حدث يُطلق عند تغيير حالة الموافقة
    /// </summary>
    event Action<TrackingConsentStatus>? ConsentStatusChanged;
}

/// <summary>
/// حالات موافقة التتبع
/// </summary>
public enum TrackingConsentStatus
{
    /// <summary>
    /// لم يتم طلب الموافقة بعد
    /// </summary>
    NotDetermined = 0,

    /// <summary>
    /// التتبع مقيد (الإعداد العام مغلق)
    /// </summary>
    Restricted = 1,

    /// <summary>
    /// المستخدم رفض التتبع
    /// </summary>
    Denied = 2,

    /// <summary>
    /// المستخدم وافق على التتبع
    /// </summary>
    Authorized = 3
}

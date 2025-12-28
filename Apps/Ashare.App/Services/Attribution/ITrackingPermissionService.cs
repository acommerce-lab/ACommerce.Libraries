namespace Ashare.App.Services.Attribution;

/// <summary>
/// حالة إذن التتبع
/// </summary>
public enum TrackingPermissionStatus
{
    /// <summary>
    /// لم يُحدد بعد (iOS 14.5+ فقط)
    /// </summary>
    NotDetermined,

    /// <summary>
    /// مقيد (الجهاز لا يسمح بالتتبع)
    /// </summary>
    Restricted,

    /// <summary>
    /// مرفوض من المستخدم
    /// </summary>
    Denied,

    /// <summary>
    /// مسموح
    /// </summary>
    Authorized,

    /// <summary>
    /// غير مطلوب (Android أو iOS قديم)
    /// </summary>
    NotRequired
}

/// <summary>
/// خدمة إدارة إذن التتبع
/// على iOS 14.5+ يجب طلب إذن قبل التتبع
/// </summary>
public interface ITrackingPermissionService
{
    /// <summary>
    /// الحصول على حالة إذن التتبع الحالية
    /// </summary>
    Task<TrackingPermissionStatus> GetStatusAsync();

    /// <summary>
    /// طلب إذن التتبع من المستخدم
    /// </summary>
    /// <returns>true إذا تم السماح</returns>
    Task<bool> RequestPermissionAsync();

    /// <summary>
    /// هل التتبع مسموح؟
    /// </summary>
    Task<bool> IsTrackingAllowedAsync();

    /// <summary>
    /// هل النظام يتطلب طلب إذن؟
    /// </summary>
    bool RequiresPermissionRequest { get; }
}

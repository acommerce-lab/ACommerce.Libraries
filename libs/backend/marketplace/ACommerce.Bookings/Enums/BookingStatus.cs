namespace ACommerce.Bookings.Enums;

/// <summary>
/// حالات الحجز
/// </summary>
public enum BookingStatus
{
    /// <summary>
    /// قيد الانتظار - تم إنشاء الحجز ولم يتم الدفع بعد
    /// </summary>
    Pending = 1,

    /// <summary>
    /// تم دفع العربون - في انتظار موافقة المالك
    /// </summary>
    DepositPaid = 2,

    /// <summary>
    /// مؤكد - وافق المالك على الحجز
    /// </summary>
    Confirmed = 3,

    /// <summary>
    /// نشط - بدأت فترة الإيجار
    /// </summary>
    Active = 4,

    /// <summary>
    /// مكتمل - انتهت فترة الإيجار بنجاح
    /// </summary>
    Completed = 5,

    /// <summary>
    /// ملغي - تم إلغاء الحجز
    /// </summary>
    Cancelled = 6,

    /// <summary>
    /// مرفوض - رفض المالك الحجز
    /// </summary>
    Rejected = 7,

    /// <summary>
    /// متنازع عليه - يوجد نزاع بين الطرفين
    /// </summary>
    Disputed = 8
}

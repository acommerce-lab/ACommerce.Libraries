namespace ACommerce.Spaces.Enums;

/// <summary>
/// حالة الحجز
/// </summary>
public enum BookingStatus
{
    /// <summary>
    /// معلق - بانتظار التأكيد
    /// </summary>
    Pending = 0,

    /// <summary>
    /// مؤكد
    /// </summary>
    Confirmed = 1,

    /// <summary>
    /// جاري - قيد الاستخدام
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// مكتمل
    /// </summary>
    Completed = 3,

    /// <summary>
    /// ملغي
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// لم يحضر
    /// </summary>
    NoShow = 5,

    /// <summary>
    /// مرفوض
    /// </summary>
    Rejected = 6,

    /// <summary>
    /// بانتظار الدفع
    /// </summary>
    AwaitingPayment = 7,

    /// <summary>
    /// مسترجع
    /// </summary>
    Refunded = 8
}

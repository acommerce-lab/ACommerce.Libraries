namespace Order.Api.Models;

/// <summary>
/// نوع استلام الطلب
/// </summary>
public enum DeliveryType
{
    /// <summary>
    /// استلام من الكاشير
    /// </summary>
    Pickup = 0,

    /// <summary>
    /// توصيل للسيارة
    /// </summary>
    CarDelivery = 1
}

/// <summary>
/// طريقة الدفع
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// دفع نقدي (كاش)
    /// </summary>
    Cash = 0,

    /// <summary>
    /// دفع بالبطاقة
    /// </summary>
    Card = 1
}

/// <summary>
/// حالة الطلب
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// طلب جديد - في انتظار قبول التاجر
    /// </summary>
    Pending = 0,

    /// <summary>
    /// تم قبول الطلب - قيد التحضير
    /// </summary>
    Accepted = 1,

    /// <summary>
    /// الطلب جاهز للاستلام
    /// </summary>
    Ready = 2,

    /// <summary>
    /// تم التسليم
    /// </summary>
    Delivered = 3,

    /// <summary>
    /// ملغي
    /// </summary>
    Cancelled = 4
}

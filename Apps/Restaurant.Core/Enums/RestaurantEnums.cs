namespace Restaurant.Core.Enums;

/// <summary>
/// حالة الرادار - تحدد ما إذا كان المطعم يستلم طلبات أم لا
/// </summary>
public enum RadarStatus
{
    /// <summary>
    /// مفتوح - يستلم طلبات جديدة
    /// </summary>
    Open = 1,

    /// <summary>
    /// مشغول - لا يستلم طلبات جديدة مؤقتاً
    /// </summary>
    Busy = 2,

    /// <summary>
    /// مغلق - خارج أوقات العمل أو أغلق صفحة الرادار
    /// </summary>
    Closed = 3
}

/// <summary>
/// حالة المطعم المحسوبة - تجمع بين الرادار وجدول الدوام
/// </summary>
public enum RestaurantAvailabilityStatus
{
    /// <summary>
    /// متاح - المطعم مفتوح ويستلم طلبات
    /// </summary>
    Available = 1,

    /// <summary>
    /// مشغول - المطعم مفتوح لكن لا يستلم طلبات جديدة حالياً
    /// </summary>
    Busy = 2,

    /// <summary>
    /// مغلق - خارج أوقات العمل
    /// </summary>
    Closed = 3,

    /// <summary>
    /// غير متاح - المطعم معلق أو محظور
    /// </summary>
    Unavailable = 4
}

/// <summary>
/// حالات طلب المطعم
/// </summary>
public enum RestaurantOrderStatus
{
    // === مرحلة العميل ===

    /// <summary>
    /// في السلة - لم يتم الطلب بعد
    /// </summary>
    Cart = 0,

    /// <summary>
    /// بانتظار رمز التحقق من العميل
    /// </summary>
    PendingConfirmation = 1,

    // === مرحلة المطعم ===

    /// <summary>
    /// ينتظر قبول المطعم (يظهر في صفحة الرادار)
    /// </summary>
    WaitingAcceptance = 2,

    /// <summary>
    /// المطعم قبل الطلب
    /// </summary>
    Accepted = 3,

    /// <summary>
    /// المطعم رفض الطلب
    /// </summary>
    Rejected = 4,

    /// <summary>
    /// قيد التحضير
    /// </summary>
    Preparing = 5,

    /// <summary>
    /// جاهز للاستلام من السائق
    /// </summary>
    Ready = 6,

    // === مرحلة السائق ===

    /// <summary>
    /// تم تعيين سائق للطلب
    /// </summary>
    AssignedToDriver = 7,

    /// <summary>
    /// السائق استلم الطلب (مسح الباركود)
    /// </summary>
    DriverPickedUp = 8,

    /// <summary>
    /// في الطريق للعميل
    /// </summary>
    OnTheWay = 9,

    // === مرحلة التسليم ===

    /// <summary>
    /// تم التسليم (رفعه السائق)
    /// </summary>
    Delivered = 10,

    /// <summary>
    /// العميل أكد الاستلام
    /// </summary>
    DeliveryConfirmed = 11,

    /// <summary>
    /// العميل نفى الاستلام - نزاع
    /// </summary>
    DeliveryDisputed = 12,

    // === حالات خاصة ===

    /// <summary>
    /// ملغي
    /// </summary>
    Cancelled = 20,

    /// <summary>
    /// مسترجع
    /// </summary>
    Refunded = 21
}

/// <summary>
/// دور الموظف في المطعم
/// </summary>
public enum EmployeeRole
{
    /// <summary>
    /// سائق توصيل
    /// </summary>
    Driver = 1,

    /// <summary>
    /// كاشير
    /// </summary>
    Cashier = 2,

    /// <summary>
    /// مدير
    /// </summary>
    Manager = 3,

    /// <summary>
    /// محضر طلبات
    /// </summary>
    Preparer = 4
}

/// <summary>
/// حالة الموظف
/// </summary>
public enum EmployeeStatus
{
    /// <summary>
    /// نشط
    /// </summary>
    Active = 1,

    /// <summary>
    /// غير نشط
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// معلق
    /// </summary>
    Suspended = 3
}

/// <summary>
/// نوع مركبة السائق
/// </summary>
public enum VehicleType
{
    /// <summary>
    /// دراجة نارية
    /// </summary>
    Motorcycle = 1,

    /// <summary>
    /// سيارة
    /// </summary>
    Car = 2,

    /// <summary>
    /// دراجة هوائية
    /// </summary>
    Bicycle = 3
}

/// <summary>
/// من قام بتغيير حالة الطلب
/// </summary>
public enum OrderStatusChangedBy
{
    Customer = 1,
    Vendor = 2,
    Driver = 3,
    System = 4,
    Admin = 5
}

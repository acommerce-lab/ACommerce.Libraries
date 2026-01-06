using ACommerce.SharedKernel.Abstractions.Entities;
using Restaurant.Core.Enums;

namespace Restaurant.Core.Entities;

/// <summary>
/// طلب المطعم - امتداد لـ Order من ACommerce.Orders
/// يحتوي على معلومات إضافية خاصة بالمطاعم
/// </summary>
public class RestaurantOrder : IBaseEntity, IAuditableEntity
{
    public Guid Id { get; set; }

    /// <summary>
    /// معرف الطلب الأصلي من ACommerce.Orders
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// معرف ملف المطعم
    /// </summary>
    public Guid RestaurantProfileId { get; set; }

    /// <summary>
    /// ملف المطعم
    /// </summary>
    public RestaurantProfile? RestaurantProfile { get; set; }

    // === رقم الطلب ===

    /// <summary>
    /// رقم الطلب المعروض (مثل: RO-2025-000001)
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// باركود الطلب (للمسح من قبل السائق)
    /// </summary>
    public string Barcode { get; set; } = string.Empty;

    // === حالة الطلب ===

    /// <summary>
    /// حالة الطلب الحالية
    /// </summary>
    public RestaurantOrderStatus Status { get; set; } = RestaurantOrderStatus.Cart;

    /// <summary>
    /// سبب الرفض (إذا تم رفض الطلب)
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// سبب الإلغاء (إذا تم إلغاء الطلب)
    /// </summary>
    public string? CancellationReason { get; set; }

    // === معلومات العميل ===

    /// <summary>
    /// معرف العميل
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// اسم العميل
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// هاتف العميل
    /// </summary>
    public string CustomerPhone { get; set; } = string.Empty;

    /// <summary>
    /// ملاحظات العميل
    /// </summary>
    public string? CustomerNotes { get; set; }

    // === عنوان التوصيل ===

    /// <summary>
    /// عنوان التوصيل الكامل
    /// </summary>
    public string DeliveryAddress { get; set; } = string.Empty;

    /// <summary>
    /// خط عرض موقع التوصيل
    /// </summary>
    public double DeliveryLatitude { get; set; }

    /// <summary>
    /// خط طول موقع التوصيل
    /// </summary>
    public double DeliveryLongitude { get; set; }

    /// <summary>
    /// تفاصيل إضافية للعنوان (رقم الشقة، علامة مميزة، إلخ)
    /// </summary>
    public string? AddressDetails { get; set; }

    // === المسافة والتوصيل ===

    /// <summary>
    /// المسافة بالكيلومتر من المطعم
    /// </summary>
    public decimal DistanceKm { get; set; }

    /// <summary>
    /// معرف نطاق التوصيل المستخدم
    /// </summary>
    public Guid? DeliveryZoneId { get; set; }

    /// <summary>
    /// نطاق التوصيل
    /// </summary>
    public DeliveryZone? DeliveryZone { get; set; }

    /// <summary>
    /// رسوم التوصيل
    /// </summary>
    public decimal DeliveryFee { get; set; }

    /// <summary>
    /// الوقت المتوقع للتوصيل بالدقائق
    /// </summary>
    public int EstimatedDeliveryMinutes { get; set; }

    // === المبالغ ===

    /// <summary>
    /// المجموع الفرعي (قبل التوصيل)
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// الخصم
    /// </summary>
    public decimal Discount { get; set; } = 0;

    /// <summary>
    /// المجموع الكلي (بعد التوصيل والخصم)
    /// </summary>
    public decimal Total { get; set; }

    // === التواريخ ===

    /// <summary>
    /// وقت إنشاء الطلب
    /// </summary>
    public DateTime OrderedAt { get; set; }

    /// <summary>
    /// وقت تأكيد الطلب بـ OTP
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// وقت قبول المطعم للطلب
    /// </summary>
    public DateTime? AcceptedAt { get; set; }

    /// <summary>
    /// وقت بدء التحضير
    /// </summary>
    public DateTime? PreparationStartedAt { get; set; }

    /// <summary>
    /// وقت جهوزية الطلب
    /// </summary>
    public DateTime? ReadyAt { get; set; }

    /// <summary>
    /// وقت استلام السائق للطلب
    /// </summary>
    public DateTime? PickedUpAt { get; set; }

    /// <summary>
    /// وقت التسليم
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// انتهاء صلاحية انتظار قبول المطعم
    /// </summary>
    public DateTime? AcceptanceExpiresAt { get; set; }

    // === السائق ===

    /// <summary>
    /// معرف السائق المعين
    /// </summary>
    public Guid? AssignedDriverId { get; set; }

    /// <summary>
    /// السائق المعين
    /// </summary>
    public VendorEmployee? AssignedDriver { get; set; }

    // === التقييم ===

    /// <summary>
    /// هل تم تقييم الطلب؟
    /// </summary>
    public bool IsRated { get; set; } = false;

    /// <summary>
    /// تقييم المطعم (1-5)
    /// </summary>
    public int? RestaurantRating { get; set; }

    /// <summary>
    /// تقييم السائق (1-5)
    /// </summary>
    public int? DriverRating { get; set; }

    /// <summary>
    /// تعليق التقييم
    /// </summary>
    public string? RatingComment { get; set; }

    // === العلاقات ===

    /// <summary>
    /// سجل تغييرات الحالة
    /// </summary>
    public List<OrderStatusHistory> StatusHistory { get; set; } = new();

    /// <summary>
    /// عناصر الطلب
    /// </summary>
    public List<RestaurantOrderItem> Items { get; set; } = new();

    // === التدقيق ===

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // === الدوال المساعدة ===

    /// <summary>
    /// الحصول على اسم الحالة بالعربية
    /// </summary>
    public string GetStatusDisplayName()
    {
        return Status switch
        {
            RestaurantOrderStatus.Cart => "في السلة",
            RestaurantOrderStatus.PendingConfirmation => "بانتظار التأكيد",
            RestaurantOrderStatus.WaitingAcceptance => "بانتظار قبول المطعم",
            RestaurantOrderStatus.Accepted => "تم القبول",
            RestaurantOrderStatus.Rejected => "مرفوض",
            RestaurantOrderStatus.Preparing => "قيد التحضير",
            RestaurantOrderStatus.Ready => "جاهز",
            RestaurantOrderStatus.AssignedToDriver => "بانتظار السائق",
            RestaurantOrderStatus.DriverPickedUp => "مع السائق",
            RestaurantOrderStatus.OnTheWay => "في الطريق",
            RestaurantOrderStatus.Delivered => "تم التسليم",
            RestaurantOrderStatus.DeliveryConfirmed => "تم تأكيد الاستلام",
            RestaurantOrderStatus.DeliveryDisputed => "نزاع على التسليم",
            RestaurantOrderStatus.Cancelled => "ملغي",
            RestaurantOrderStatus.Refunded => "مسترجع",
            _ => "غير معروف"
        };
    }

    /// <summary>
    /// الحصول على لون الحالة
    /// </summary>
    public string GetStatusColor()
    {
        return Status switch
        {
            RestaurantOrderStatus.Cart => "#6B7280",
            RestaurantOrderStatus.PendingConfirmation => "#F59E0B",
            RestaurantOrderStatus.WaitingAcceptance => "#F59E0B",
            RestaurantOrderStatus.Accepted => "#3B82F6",
            RestaurantOrderStatus.Rejected => "#EF4444",
            RestaurantOrderStatus.Preparing => "#8B5CF6",
            RestaurantOrderStatus.Ready => "#10B981",
            RestaurantOrderStatus.AssignedToDriver => "#3B82F6",
            RestaurantOrderStatus.DriverPickedUp => "#3B82F6",
            RestaurantOrderStatus.OnTheWay => "#F97316",
            RestaurantOrderStatus.Delivered => "#22C55E",
            RestaurantOrderStatus.DeliveryConfirmed => "#22C55E",
            RestaurantOrderStatus.DeliveryDisputed => "#EF4444",
            RestaurantOrderStatus.Cancelled => "#6B7280",
            RestaurantOrderStatus.Refunded => "#6B7280",
            _ => "#6B7280"
        };
    }

    /// <summary>
    /// هل يمكن للعميل إلغاء الطلب؟
    /// </summary>
    public bool CanCustomerCancel()
    {
        return Status is RestaurantOrderStatus.PendingConfirmation
            or RestaurantOrderStatus.WaitingAcceptance;
    }

    /// <summary>
    /// هل يمكن للمطعم قبول الطلب؟
    /// </summary>
    public bool CanVendorAccept()
    {
        return Status == RestaurantOrderStatus.WaitingAcceptance;
    }

    /// <summary>
    /// هل الطلب نشط (لم ينتهِ بعد)؟
    /// </summary>
    public bool IsActive()
    {
        return Status is not (RestaurantOrderStatus.Delivered
            or RestaurantOrderStatus.DeliveryConfirmed
            or RestaurantOrderStatus.DeliveryDisputed
            or RestaurantOrderStatus.Cancelled
            or RestaurantOrderStatus.Refunded
            or RestaurantOrderStatus.Rejected);
    }

    /// <summary>
    /// توليد رقم طلب جديد
    /// </summary>
    public static string GenerateOrderNumber()
    {
        var year = DateTime.Now.Year;
        var random = new Random();
        var number = random.Next(100000, 999999);
        return $"RO-{year}-{number}";
    }

    /// <summary>
    /// توليد باركود جديد
    /// </summary>
    public static string GenerateBarcode()
    {
        return Guid.NewGuid().ToString("N")[..12].ToUpper();
    }
}

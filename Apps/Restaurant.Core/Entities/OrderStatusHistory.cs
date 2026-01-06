using ACommerce.SharedKernel.Abstractions.Entities;
using Restaurant.Core.Enums;

namespace Restaurant.Core.Entities;

/// <summary>
/// سجل تتبع تغييرات حالة الطلب
/// </summary>
public class OrderStatusHistory : IBaseEntity
{
    public Guid Id { get; set; }

    /// <summary>
    /// معرف الطلب
    /// </summary>
    public Guid RestaurantOrderId { get; set; }

    /// <summary>
    /// الطلب
    /// </summary>
    public RestaurantOrder? RestaurantOrder { get; set; }

    /// <summary>
    /// الحالة السابقة
    /// </summary>
    public RestaurantOrderStatus FromStatus { get; set; }

    /// <summary>
    /// الحالة الجديدة
    /// </summary>
    public RestaurantOrderStatus ToStatus { get; set; }

    /// <summary>
    /// وقت التغيير
    /// </summary>
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// من قام بالتغيير
    /// </summary>
    public OrderStatusChangedBy ChangedBy { get; set; }

    /// <summary>
    /// معرف المستخدم الذي قام بالتغيير
    /// </summary>
    public string? ChangedByUserId { get; set; }

    /// <summary>
    /// اسم المستخدم الذي قام بالتغيير
    /// </summary>
    public string? ChangedByName { get; set; }

    /// <summary>
    /// ملاحظات إضافية
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// خط العرض عند التغيير (مفيد للسائق)
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// خط الطول عند التغيير (مفيد للسائق)
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// بيانات إضافية (JSON)
    /// </summary>
    public string? Metadata { get; set; }

    // === الدوال المساعدة ===

    /// <summary>
    /// الحصول على وصف التغيير
    /// </summary>
    public string GetChangeDescription()
    {
        var fromName = GetStatusName(FromStatus);
        var toName = GetStatusName(ToStatus);
        var byName = GetChangedByName();

        return $"تم تغيير الحالة من \"{fromName}\" إلى \"{toName}\" بواسطة {byName}";
    }

    /// <summary>
    /// الحصول على اسم الحالة بالعربية
    /// </summary>
    private static string GetStatusName(RestaurantOrderStatus status)
    {
        return status switch
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
            RestaurantOrderStatus.DeliveryDisputed => "نزاع",
            RestaurantOrderStatus.Cancelled => "ملغي",
            RestaurantOrderStatus.Refunded => "مسترجع",
            _ => "غير معروف"
        };
    }

    /// <summary>
    /// الحصول على اسم من قام بالتغيير
    /// </summary>
    private string GetChangedByName()
    {
        if (!string.IsNullOrEmpty(ChangedByName))
        {
            return ChangedByName;
        }

        return ChangedBy switch
        {
            OrderStatusChangedBy.Customer => "العميل",
            OrderStatusChangedBy.Vendor => "المطعم",
            OrderStatusChangedBy.Driver => "السائق",
            OrderStatusChangedBy.System => "النظام",
            OrderStatusChangedBy.Admin => "الإدارة",
            _ => "غير معروف"
        };
    }

    /// <summary>
    /// إنشاء سجل تغيير جديد
    /// </summary>
    public static OrderStatusHistory Create(
        Guid orderId,
        RestaurantOrderStatus fromStatus,
        RestaurantOrderStatus toStatus,
        OrderStatusChangedBy changedBy,
        string? userId = null,
        string? userName = null,
        string? notes = null,
        double? latitude = null,
        double? longitude = null)
    {
        return new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            RestaurantOrderId = orderId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = changedBy,
            ChangedByUserId = userId,
            ChangedByName = userName,
            Notes = notes,
            Latitude = latitude,
            Longitude = longitude
        };
    }
}

using ACommerce.Contracts.Interfaces;
using Restaurant.Core.Enums;

namespace Restaurant.Core.Entities;

/// <summary>
/// الطلب
/// </summary>
public class Order : IBaseEntity, IAuditableEntity
{
    public Guid Id { get; set; }

    /// <summary>
    /// رقم الطلب (للعرض)
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// معرف العميل
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// اسم العميل
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// هاتف العميل
    /// </summary>
    public string CustomerPhone { get; set; } = string.Empty;

    /// <summary>
    /// معرف المطعم
    /// </summary>
    public Guid RestaurantId { get; set; }

    /// <summary>
    /// اسم المطعم
    /// </summary>
    public string RestaurantName { get; set; } = string.Empty;

    /// <summary>
    /// معرف السائق
    /// </summary>
    public Guid? DriverId { get; set; }

    /// <summary>
    /// اسم السائق
    /// </summary>
    public string? DriverName { get; set; }

    /// <summary>
    /// هاتف السائق
    /// </summary>
    public string? DriverPhone { get; set; }

    /// <summary>
    /// حالة الطلب
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// عناصر الطلب
    /// </summary>
    public List<OrderItem> Items { get; set; } = new();

    /// <summary>
    /// المجموع الفرعي
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// رسوم التوصيل
    /// </summary>
    public decimal DeliveryFee { get; set; }

    /// <summary>
    /// المجموع الكلي
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// ملاحظات العميل
    /// </summary>
    public string? CustomerNotes { get; set; }

    /// <summary>
    /// ملاحظات المطعم
    /// </summary>
    public string? RestaurantNotes { get; set; }

    /// <summary>
    /// عنوان التوصيل
    /// </summary>
    public DeliveryAddress DeliveryAddress { get; set; } = new();

    /// <summary>
    /// الوقت المتوقع للتوصيل بالدقائق
    /// </summary>
    public int EstimatedDeliveryMinutes { get; set; }

    /// <summary>
    /// وقت الاستلام الفعلي من المطعم
    /// </summary>
    public DateTime? PickedUpAt { get; set; }

    /// <summary>
    /// وقت التوصيل الفعلي
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// سبب الإلغاء
    /// </summary>
    public string? CancellationReason { get; set; }

    /// <summary>
    /// من قام بالإلغاء
    /// </summary>
    public string? CancelledBy { get; set; }

    /// <summary>
    /// سلسلة المستندات
    /// </summary>
    public List<OrderDocument> Documents { get; set; } = new();

    /// <summary>
    /// سجل الحالات
    /// </summary>
    public List<OrderStatusHistory> StatusHistory { get; set; } = new();

    /// <summary>
    /// تاريخ الإنشاء
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// تاريخ التحديث
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// عنصر في الطلب
/// </summary>
public class OrderItem : IBaseEntity
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// تفاصيل الإضافات
    /// </summary>
    public string? AddonsJson { get; set; }

    /// <summary>
    /// تفاصيل الخيارات
    /// </summary>
    public string? OptionsJson { get; set; }
}

/// <summary>
/// عنوان التوصيل
/// </summary>
public class DeliveryAddress
{
    public string Label { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string? BuildingNumber { get; set; }
    public string? Floor { get; set; }
    public string? Apartment { get; set; }
    public string? Landmark { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

/// <summary>
/// سجل تغيير حالة الطلب
/// </summary>
public class OrderStatusHistory
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public string? Notes { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string ChangedByType { get; set; } = string.Empty; // Customer, Vendor, Driver, System
    public DateTime ChangedAt { get; set; }
}

/// <summary>
/// مستند في سلسلة المستندات
/// </summary>
public class OrderDocument
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }

    /// <summary>
    /// نوع المستند
    /// </summary>
    public OrderDocumentType Type { get; set; }

    /// <summary>
    /// بيانات المستند (JSON)
    /// </summary>
    public string DataJson { get; set; } = string.Empty;

    /// <summary>
    /// التوقيع الرقمي
    /// </summary>
    public string? Signature { get; set; }

    /// <summary>
    /// من أنشأ المستند
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// تاريخ الإنشاء
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// أنواع المستندات
/// </summary>
public enum OrderDocumentType
{
    /// <summary>
    /// طلب العميل
    /// </summary>
    CustomerOrderRequest = 1,

    /// <summary>
    /// قبول المطعم
    /// </summary>
    RestaurantAcceptance = 2,

    /// <summary>
    /// تعيين السائق
    /// </summary>
    DriverAssignment = 3,

    /// <summary>
    /// استلام من المطعم
    /// </summary>
    PickupConfirmation = 4,

    /// <summary>
    /// تسليم للعميل
    /// </summary>
    DeliveryConfirmation = 5,

    /// <summary>
    /// إلغاء
    /// </summary>
    Cancellation = 6
}

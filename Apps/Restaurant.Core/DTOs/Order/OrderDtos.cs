using Restaurant.Core.Enums;

namespace Restaurant.Core.DTOs.Order;

/// <summary>
/// بيانات الطلب الكاملة
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public Guid? DriverId { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Total { get; set; }
    public string? CustomerNotes { get; set; }
    public string? RestaurantNotes { get; set; }
    public DeliveryAddressDto DeliveryAddress { get; set; } = new();
    public int EstimatedDeliveryMinutes { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// عنصر في الطلب
/// </summary>
public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
    public decimal TotalPrice { get; set; }
    public List<OrderItemAddonDto>? Addons { get; set; }
    public List<OrderItemOptionDto>? Options { get; set; }
}

public class OrderItemAddonDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class OrderItemOptionDto
{
    public string GroupName { get; set; } = string.Empty;
    public string ValueName { get; set; } = string.Empty;
    public decimal PriceDifference { get; set; }
}

/// <summary>
/// عنوان التوصيل
/// </summary>
public class DeliveryAddressDto
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
/// ملخص الطلب (للقوائم)
/// </summary>
public class OrderSummaryDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string RestaurantName { get; set; } = string.Empty;
    public string? RestaurantImageUrl { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public int ItemsCount { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public int EstimatedMinutes { get; set; }
}

/// <summary>
/// طلب إنشاء طلب جديد
/// </summary>
public class CreateOrderRequest
{
    public Guid RestaurantId { get; set; }
    public Guid CartId { get; set; }
    public DeliveryAddressDto DeliveryAddress { get; set; } = new();
    public string? Notes { get; set; }
}

/// <summary>
/// طلب الرادار (للمطعم)
/// </summary>
public class RadarOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal Total { get; set; }
    public string? Notes { get; set; }
    public DeliveryAddressDto DeliveryAddress { get; set; } = new();
    public double DistanceKm { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// الوقت المتبقي لقبول الطلب (بالثواني)
    /// </summary>
    public int SecondsToAccept { get; set; }
}

/// <summary>
/// رد المطعم على الطلب
/// </summary>
public class RestaurantOrderResponse
{
    /// <summary>
    /// قبول أم رفض
    /// </summary>
    public bool Accepted { get; set; }

    /// <summary>
    /// سبب الرفض
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// وقت التحضير المتوقع (بالدقائق)
    /// </summary>
    public int? PreparationMinutes { get; set; }

    /// <summary>
    /// ملاحظات للعميل
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// طلب السائق
/// </summary>
public class DriverOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;

    // معلومات المطعم
    public string RestaurantName { get; set; } = string.Empty;
    public string RestaurantPhone { get; set; } = string.Empty;
    public double RestaurantLatitude { get; set; }
    public double RestaurantLongitude { get; set; }
    public string RestaurantAddress { get; set; } = string.Empty;

    // معلومات العميل
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DeliveryAddressDto CustomerAddress { get; set; } = new();

    // تفاصيل
    public int ItemsCount { get; set; }
    public decimal Total { get; set; }
    public decimal DeliveryFee { get; set; }

    /// <summary>
    /// عمولة السائق
    /// </summary>
    public decimal DriverEarnings { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? PickedUpAt { get; set; }

    /// <summary>
    /// هل الطلب جاهز للاستلام؟
    /// </summary>
    public bool IsReadyForPickup { get; set; }
}

/// <summary>
/// تتبع الطلب (للعميل)
/// </summary>
public class OrderTrackingDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;

    /// <summary>
    /// مراحل الطلب
    /// </summary>
    public List<OrderStageDto> Stages { get; set; } = new();

    /// <summary>
    /// موقع السائق الحالي
    /// </summary>
    public DriverLocationDto? DriverLocation { get; set; }

    /// <summary>
    /// الوقت المتوقع للوصول
    /// </summary>
    public int? EstimatedMinutes { get; set; }

    /// <summary>
    /// رسالة الحالة
    /// </summary>
    public string StatusMessage { get; set; } = string.Empty;
}

/// <summary>
/// مرحلة في الطلب
/// </summary>
public class OrderStageDto
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// موقع السائق
/// </summary>
public class DriverLocationDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// تحديث حالة الطلب
/// </summary>
public class UpdateOrderStatusRequest
{
    public OrderStatus NewStatus { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// إلغاء الطلب
/// </summary>
public class CancelOrderRequest
{
    public string Reason { get; set; } = string.Empty;
}

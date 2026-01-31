namespace Order.Shared.Models;

/// <summary>
/// نوع التوصيل
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
    /// نقدي
    /// </summary>
    Cash = 0,

    /// <summary>
    /// بطاقة
    /// </summary>
    Card = 1
}

/// <summary>
/// موقع التوصيل
/// </summary>
public class DeliveryLocationDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? LocationDescription { get; set; }
    public bool IsLiveLocation { get; set; }
}

/// <summary>
/// معلومات السيارة
/// </summary>
public class CarInfoDto
{
    public string? CarModel { get; set; }
    public string? CarColor { get; set; }
    public string? PlateNumber { get; set; }
}

/// <summary>
/// طلب إنشاء طلب جديد
/// </summary>
public class CreateOrderRequest
{
    public List<OrderItemRequest> Items { get; set; } = new();
    public DeliveryType DeliveryType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? CustomerNotes { get; set; }
    public DeliveryLocationDto? DeliveryLocation { get; set; }
    public CarInfoDto? CarInfo { get; set; }
}

/// <summary>
/// عنصر في الطلب
/// </summary>
public class OrderItemRequest
{
    public Guid OfferId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Notes { get; set; }
}

/// <summary>
/// نموذج الطلب
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }
    public string? OrderNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; }
    public string? StatusAr { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
    public DeliveryType DeliveryType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DeliveryLocationDto? DeliveryLocation { get; set; }
    public CarInfoDto? CarInfo { get; set; }
    public VendorDto? Vendor { get; set; }
    public List<OrderItemDto>? Items { get; set; }
}

/// <summary>
/// عنصر في الطلب (للعرض)
/// </summary>
public class OrderItemDto
{
    public Guid Id { get; set; }
    public string? OfferTitle { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// استجابة قائمة الطلبات
/// </summary>
public class OrdersResponse
{
    public List<OrderDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

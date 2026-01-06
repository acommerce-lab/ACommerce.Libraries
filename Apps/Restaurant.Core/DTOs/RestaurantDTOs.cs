using Restaurant.Core.Enums;

namespace Restaurant.Core.DTOs;

// === DTOs للمطعم ===

/// <summary>
/// معلومات المطعم للعرض في القائمة
/// </summary>
public class RestaurantListItemDto
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string CuisineType { get; set; } = string.Empty;
    public string? Description { get; set; }

    // الحالة
    public RestaurantAvailabilityStatus AvailabilityStatus { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public bool CanOrder { get; set; }

    // التقييم
    public decimal? Rating { get; set; }
    public int ReviewCount { get; set; }

    // التوصيل
    public decimal? DistanceKm { get; set; }
    public decimal? DeliveryFee { get; set; }
    public string? DeliveryFeeDisplay { get; set; }
    public string? EstimatedTime { get; set; }

    // الحد الأدنى
    public decimal MinimumOrderAmount { get; set; }
}

/// <summary>
/// تفاصيل المطعم الكاملة
/// </summary>
public class RestaurantDetailsDto : RestaurantListItemDto
{
    public string? FullAddress { get; set; }
    public string? City { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public int AveragePreparationTime { get; set; }
    public bool SupportsDelivery { get; set; }
    public bool SupportsPickup { get; set; }

    // ساعات العمل
    public List<ScheduleDto> WeeklySchedule { get; set; } = new();

    // نطاقات التوصيل
    public List<DeliveryZoneDto> DeliveryZones { get; set; } = new();
}

/// <summary>
/// جدول الدوام
/// </summary>
public class ScheduleDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public string DayName { get; set; } = string.Empty;
    public bool IsOpen { get; set; }
    public string? OpenTime { get; set; }
    public string? CloseTime { get; set; }
    public string? BreakTime { get; set; }
    public string FormattedHours { get; set; } = string.Empty;
}

/// <summary>
/// نطاق التوصيل
/// </summary>
public class DeliveryZoneDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MinRadiusKm { get; set; }
    public decimal MaxRadiusKm { get; set; }
    public decimal DeliveryFee { get; set; }
    public string DeliveryFeeDisplay { get; set; } = string.Empty;
    public int EstimatedMinutesMin { get; set; }
    public int EstimatedMinutesMax { get; set; }
    public string EstimatedTimeDisplay { get; set; } = string.Empty;
    public bool IsCustomerInThisZone { get; set; }
}

// === DTOs للطلبات ===

/// <summary>
/// إنشاء طلب جديد
/// </summary>
public class CreateOrderDto
{
    public Guid RestaurantId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public string DeliveryAddress { get; set; } = string.Empty;
    public double DeliveryLatitude { get; set; }
    public double DeliveryLongitude { get; set; }
    public string? AddressDetails { get; set; }
    public string? CustomerNotes { get; set; }
}

/// <summary>
/// عنصر في الطلب
/// </summary>
public class OrderItemDto
{
    public Guid ListingId { get; set; }
    public int Quantity { get; set; }
    public string? SelectedOptions { get; set; }
    public string? SelectedAddons { get; set; }
    public string? SpecialInstructions { get; set; }
}

/// <summary>
/// معلومات الطلب للعرض
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;

    // الحالة
    public RestaurantOrderStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;

    // المطعم
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public string? RestaurantLogoUrl { get; set; }

    // العميل
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerNotes { get; set; }

    // العنوان
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? AddressDetails { get; set; }
    public double DeliveryLatitude { get; set; }
    public double DeliveryLongitude { get; set; }

    // المبالغ
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }

    // التوقيت
    public DateTime OrderedAt { get; set; }
    public int EstimatedDeliveryMinutes { get; set; }
    public DateTime? EstimatedDeliveryTime { get; set; }

    // السائق
    public DriverInfoDto? Driver { get; set; }

    // العناصر
    public List<OrderItemDisplayDto> Items { get; set; } = new();
}

/// <summary>
/// عنصر الطلب للعرض
/// </summary>
public class OrderItemDisplayDto
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
    public string? SpecialInstructions { get; set; }
    public List<string>? Options { get; set; }
    public List<string>? Addons { get; set; }
}

/// <summary>
/// معلومات السائق
/// </summary>
public class DriverInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string? VehicleType { get; set; }
    public string? VehiclePlate { get; set; }
    public decimal? Rating { get; set; }
    public double? CurrentLatitude { get; set; }
    public double? CurrentLongitude { get; set; }
}

// === DTOs للرادار ===

/// <summary>
/// طلب جديد في صفحة الرادار
/// </summary>
public class RadarOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderedAt { get; set; }
    public int SecondsAgo { get; set; }

    // العميل
    public string CustomerName { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }

    // المبالغ
    public decimal Total { get; set; }
    public int ItemsCount { get; set; }

    // العناصر (ملخص)
    public List<string> ItemsSummary { get; set; } = new();
    public string? CustomerNotes { get; set; }

    // الوقت المتبقي للقبول
    public int SecondsUntilExpiry { get; set; }
}

/// <summary>
/// حالة الرادار
/// </summary>
public class RadarStatusDto
{
    public RadarStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public DateTime? OpenedAt { get; set; }
    public int PendingOrdersCount { get; set; }
    public int PreparingOrdersCount { get; set; }
    public int ReadyOrdersCount { get; set; }
}

// === DTOs للموظفين ===

/// <summary>
/// إنشاء موظف جديد
/// </summary>
public class CreateEmployeeDto
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public EmployeeRole Role { get; set; }
    public VehicleType? VehicleType { get; set; }
    public string? VehiclePlate { get; set; }
    public int MaxConcurrentOrders { get; set; } = 3;
}

/// <summary>
/// معلومات الموظف
/// </summary>
public class EmployeeDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhotoUrl { get; set; }

    public EmployeeRole Role { get; set; }
    public string RoleDisplay { get; set; } = string.Empty;

    public EmployeeStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;

    // للسائق فقط
    public bool IsDriver { get; set; }
    public bool IsAvailable { get; set; }
    public string? VehicleType { get; set; }
    public string? VehiclePlate { get; set; }
    public int CurrentOrdersCount { get; set; }
    public int MaxConcurrentOrders { get; set; }

    // الإحصائيات
    public int TotalDeliveries { get; set; }
    public decimal? AverageRating { get; set; }
}

// === DTOs للتوصيل ===

/// <summary>
/// حساب تكلفة التوصيل
/// </summary>
public class DeliveryCalculationRequestDto
{
    public Guid RestaurantId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

/// <summary>
/// نتيجة حساب التوصيل
/// </summary>
public class DeliveryCalculationResponseDto
{
    public bool IsAvailable { get; set; }
    public decimal DistanceKm { get; set; }
    public decimal DeliveryFee { get; set; }
    public string DeliveryFeeDisplay { get; set; } = string.Empty;
    public int EstimatedMinutesMin { get; set; }
    public int EstimatedMinutesMax { get; set; }
    public string EstimatedTimeDisplay { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

// === DTOs للسلة ===

/// <summary>
/// السلة
/// </summary>
public class CartDto
{
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public string? RestaurantLogoUrl { get; set; }

    // حالة المطعم
    public RestaurantAvailabilityStatus RestaurantStatus { get; set; }
    public string RestaurantStatusMessage { get; set; } = string.Empty;
    public bool CanCheckout { get; set; }
    public string? CannotCheckoutReason { get; set; }

    // العناصر
    public List<CartItemDto> Items { get; set; } = new();

    // المبالغ
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }

    // الحد الأدنى
    public decimal MinimumOrderAmount { get; set; }
    public decimal RemainingForMinimum { get; set; }
    public bool MeetsMinimumOrder { get; set; }
}

/// <summary>
/// عنصر في السلة
/// </summary>
public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ExtrasPrice { get; set; }
    public decimal Total { get; set; }
    public string? SpecialInstructions { get; set; }
}

/// <summary>
/// إضافة عنصر للسلة
/// </summary>
public class AddToCartDto
{
    public Guid RestaurantId { get; set; }
    public Guid ListingId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? SelectedOptions { get; set; }
    public string? SelectedAddons { get; set; }
    public string? SpecialInstructions { get; set; }
}

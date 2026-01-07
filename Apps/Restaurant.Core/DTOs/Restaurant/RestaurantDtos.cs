using Restaurant.Core.Enums;

namespace Restaurant.Core.DTOs.Restaurant;

/// <summary>
/// بيانات المطعم للعرض
/// </summary>
public class RestaurantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public string CuisineType { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public RestaurantAvailabilityStatus AvailabilityStatus { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public string? StatusMessage { get; set; }
    public decimal Rating { get; set; }
    public int RatingsCount { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public int AveragePreparationMinutes { get; set; }
    public bool CanOrder { get; set; }
    public List<WorkingHoursDto> WorkingHours { get; set; } = new();
}

/// <summary>
/// ملخص المطعم (للقوائم)
/// </summary>
public class RestaurantSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string CuisineType { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public RestaurantAvailabilityStatus AvailabilityStatus { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public decimal DeliveryFee { get; set; }
    public int EstimatedMinutes { get; set; }
    public double DistanceKm { get; set; }
    public bool CanOrder { get; set; }
}

/// <summary>
/// أوقات العمل
/// </summary>
public class WorkingHoursDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public string DayName { get; set; } = string.Empty;
    public string OpenTime { get; set; } = string.Empty;
    public string CloseTime { get; set; } = string.Empty;
    public bool IsClosed { get; set; }
    public bool IsToday { get; set; }
}

/// <summary>
/// منطقة التوصيل
/// </summary>
public class DeliveryZoneDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double MinDistanceKm { get; set; }
    public double MaxDistanceKm { get; set; }
    public decimal DeliveryFee { get; set; }
    public int AdditionalMinutes { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// طلب البحث عن المطاعم
/// </summary>
public class SearchRestaurantsRequest
{
    /// <summary>
    /// نص البحث
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// نوع المطبخ
    /// </summary>
    public string? CuisineType { get; set; }

    /// <summary>
    /// موقع المستخدم - خط العرض
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// موقع المستخدم - خط الطول
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// الحد الأقصى للمسافة (كم)
    /// </summary>
    public double? MaxDistanceKm { get; set; }

    /// <summary>
    /// إظهار المتاحين فقط
    /// </summary>
    public bool? AvailableOnly { get; set; }

    /// <summary>
    /// الترتيب حسب
    /// </summary>
    public RestaurantSortBy SortBy { get; set; } = RestaurantSortBy.Distance;

    /// <summary>
    /// رقم الصفحة
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// حجم الصفحة
    /// </summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// ترتيب المطاعم
/// </summary>
public enum RestaurantSortBy
{
    Distance,
    Rating,
    DeliveryTime,
    MinimumOrder
}

/// <summary>
/// نتيجة البحث
/// </summary>
public class RestaurantSearchResultDto
{
    public List<RestaurantSummaryDto> Restaurants { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// أنواع المطابخ المتاحة
/// </summary>
public class CuisineTypeDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Icon { get; set; }
    public int RestaurantsCount { get; set; }
}

/// <summary>
/// إعدادات المطعم (للبائع)
/// </summary>
public class RestaurantSettingsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public string CuisineType { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public int AveragePreparationMinutes { get; set; }
    public bool SupportsDelivery { get; set; }
    public bool SupportsPickup { get; set; }
    public List<WorkingHoursDto> WorkingHours { get; set; } = new();
    public List<DeliveryZoneDto> DeliveryZones { get; set; } = new();
}

/// <summary>
/// تحديث إعدادات المطعم
/// </summary>
public class UpdateRestaurantSettingsRequest
{
    public string? Name { get; set; }
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public string? CuisineType { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public int? AveragePreparationMinutes { get; set; }
    public bool? SupportsDelivery { get; set; }
    public bool? SupportsPickup { get; set; }
}

/// <summary>
/// تحديث حالة الرادار
/// </summary>
public class UpdateRadarStatusRequest
{
    public RadarStatus Status { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// حالة الرادار
/// </summary>
public enum RadarStatus
{
    /// <summary>
    /// مغلق
    /// </summary>
    Closed = 0,

    /// <summary>
    /// مفتوح ويستقبل طلبات
    /// </summary>
    Open = 1,

    /// <summary>
    /// مشغول (طلبات كثيرة)
    /// </summary>
    Busy = 2
}

using ACommerce.SharedKernel.Abstractions.Entities;
using Restaurant.Core.Enums;

namespace Restaurant.Core.Entities;

/// <summary>
/// ملف تعريف المطعم - معلومات إضافية خاصة بالمطاعم
/// مرتبط بـ Vendor من ACommerce.Vendors
/// </summary>
public class RestaurantProfile : IBaseEntity, IAuditableEntity
{
    public Guid Id { get; set; }

    /// <summary>
    /// معرف البائع من ACommerce.Vendors
    /// </summary>
    public Guid VendorId { get; set; }

    // === معلومات المطعم ===

    /// <summary>
    /// نوع المطبخ (سعودي، هندي، إيطالي، وجبات سريعة، إلخ)
    /// </summary>
    public string CuisineType { get; set; } = string.Empty;

    /// <summary>
    /// وصف مختصر للمطعم
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// متوسط وقت التحضير بالدقائق
    /// </summary>
    public int AveragePreparationTime { get; set; } = 20;

    /// <summary>
    /// الحد الأدنى لقيمة الطلب
    /// </summary>
    public decimal MinimumOrderAmount { get; set; } = 0;

    /// <summary>
    /// هل يدعم التوصيل؟
    /// </summary>
    public bool SupportsDelivery { get; set; } = true;

    /// <summary>
    /// هل يدعم الاستلام من المطعم؟
    /// </summary>
    public bool SupportsPickup { get; set; } = false;

    // === حالة الرادار ===

    /// <summary>
    /// حالة استلام الطلبات الحالية
    /// </summary>
    public RadarStatus CurrentRadarStatus { get; set; } = RadarStatus.Closed;

    /// <summary>
    /// وقت فتح صفحة الرادار
    /// </summary>
    public DateTime? RadarOpenedAt { get; set; }

    /// <summary>
    /// وقت آخر تحديث لحالة الرادار
    /// </summary>
    public DateTime? RadarLastUpdatedAt { get; set; }

    // === الموقع ===

    /// <summary>
    /// خط العرض
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// خط الطول
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// العنوان الكامل
    /// </summary>
    public string? FullAddress { get; set; }

    /// <summary>
    /// المدينة
    /// </summary>
    public string? City { get; set; }

    // === العلاقات ===

    /// <summary>
    /// جدول الدوام الأسبوعي
    /// </summary>
    public List<VendorSchedule> WeeklySchedule { get; set; } = new();

    /// <summary>
    /// نطاقات التوصيل
    /// </summary>
    public List<DeliveryZone> DeliveryZones { get; set; } = new();

    /// <summary>
    /// موظفي المطعم
    /// </summary>
    public List<VendorEmployee> Employees { get; set; } = new();

    // === التدقيق ===

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // === الدوال المساعدة ===

    /// <summary>
    /// حساب حالة المطعم الفعلية
    /// تجمع بين حالة الرادار وجدول الدوام
    /// </summary>
    public RestaurantAvailabilityStatus GetAvailabilityStatus()
    {
        // التحقق من جدول الدوام أولاً
        var now = DateTime.Now;
        var todaySchedule = WeeklySchedule.FirstOrDefault(s => s.DayOfWeek == now.DayOfWeek);

        if (todaySchedule == null || !todaySchedule.IsOpen)
        {
            return RestaurantAvailabilityStatus.Closed;
        }

        var currentTime = now.TimeOfDay;

        // التحقق من ساعات العمل
        if (currentTime < todaySchedule.OpenTime || currentTime > todaySchedule.CloseTime)
        {
            return RestaurantAvailabilityStatus.Closed;
        }

        // التحقق من فترة الاستراحة
        if (todaySchedule.BreakStartTime.HasValue && todaySchedule.BreakEndTime.HasValue)
        {
            if (currentTime >= todaySchedule.BreakStartTime.Value &&
                currentTime <= todaySchedule.BreakEndTime.Value)
            {
                return RestaurantAvailabilityStatus.Closed;
            }
        }

        // الآن نتحقق من حالة الرادار
        return CurrentRadarStatus switch
        {
            RadarStatus.Open => RestaurantAvailabilityStatus.Available,
            RadarStatus.Busy => RestaurantAvailabilityStatus.Busy,
            RadarStatus.Closed => RestaurantAvailabilityStatus.Closed,
            _ => RestaurantAvailabilityStatus.Unavailable
        };
    }

    /// <summary>
    /// هل يمكن للعميل الطلب الآن؟
    /// </summary>
    public bool CanAcceptOrders()
    {
        return GetAvailabilityStatus() == RestaurantAvailabilityStatus.Available;
    }

    /// <summary>
    /// الحصول على رسالة الحالة للعرض
    /// </summary>
    public (string Message, string Color) GetStatusDisplay()
    {
        var status = GetAvailabilityStatus();
        return status switch
        {
            RestaurantAvailabilityStatus.Available => ("متاح", "#22C55E"),
            RestaurantAvailabilityStatus.Busy => ("مشغول", "#F59E0B"),
            RestaurantAvailabilityStatus.Closed => ("مغلق", "#EF4444"),
            RestaurantAvailabilityStatus.Unavailable => ("غير متاح", "#6B7280"),
            _ => ("غير معروف", "#6B7280")
        };
    }
}

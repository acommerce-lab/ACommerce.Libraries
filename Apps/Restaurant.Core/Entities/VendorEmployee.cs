using ACommerce.SharedKernel.Abstractions.Entities;
using Restaurant.Core.Enums;

namespace Restaurant.Core.Entities;

/// <summary>
/// موظف المطعم (سائق، كاشير، مدير، محضر)
/// </summary>
public class VendorEmployee : IBaseEntity, IAuditableEntity
{
    public Guid Id { get; set; }

    /// <summary>
    /// معرف ملف المطعم
    /// </summary>
    public Guid RestaurantProfileId { get; set; }

    /// <summary>
    /// ملف المطعم
    /// </summary>
    public RestaurantProfile? RestaurantProfile { get; set; }

    /// <summary>
    /// معرف المستخدم من نظام المصادقة
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    // === المعلومات الشخصية ===

    /// <summary>
    /// الاسم الكامل
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// رقم الهاتف
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// البريد الإلكتروني (اختياري)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// صورة الموظف
    /// </summary>
    public string? PhotoUrl { get; set; }

    // === الوظيفة ===

    /// <summary>
    /// دور الموظف
    /// </summary>
    public EmployeeRole Role { get; set; }

    /// <summary>
    /// حالة الموظف
    /// </summary>
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

    /// <summary>
    /// تاريخ بداية العمل
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// تاريخ نهاية العمل
    /// </summary>
    public DateTime? EndDate { get; set; }

    // === معلومات السائق فقط ===

    /// <summary>
    /// نوع المركبة (للسائق فقط)
    /// </summary>
    public VehicleType? VehicleType { get; set; }

    /// <summary>
    /// رقم لوحة المركبة (للسائق فقط)
    /// </summary>
    public string? VehiclePlate { get; set; }

    /// <summary>
    /// هل السائق متاح للطلبات؟
    /// </summary>
    public bool IsAvailable { get; set; } = false;

    /// <summary>
    /// خط العرض الحالي (للسائق)
    /// </summary>
    public double? CurrentLatitude { get; set; }

    /// <summary>
    /// خط الطول الحالي (للسائق)
    /// </summary>
    public double? CurrentLongitude { get; set; }

    /// <summary>
    /// آخر تحديث للموقع
    /// </summary>
    public DateTime? LastLocationUpdate { get; set; }

    /// <summary>
    /// عدد الطلبات الحالية التي يحملها السائق
    /// </summary>
    public int CurrentOrdersCount { get; set; } = 0;

    /// <summary>
    /// الحد الأقصى للطلبات التي يمكن أن يحملها في وقت واحد
    /// </summary>
    public int MaxConcurrentOrders { get; set; } = 3;

    // === الإحصائيات ===

    /// <summary>
    /// إجمالي عدد التوصيلات
    /// </summary>
    public int TotalDeliveries { get; set; } = 0;

    /// <summary>
    /// متوسط التقييم
    /// </summary>
    public decimal? AverageRating { get; set; }

    /// <summary>
    /// عدد التقييمات
    /// </summary>
    public int RatingCount { get; set; } = 0;

    // === التدقيق ===

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // === الدوال المساعدة ===

    /// <summary>
    /// هل هو سائق؟
    /// </summary>
    public bool IsDriver => Role == EmployeeRole.Driver;

    /// <summary>
    /// هل السائق يمكنه استلام طلبات جديدة؟
    /// </summary>
    public bool CanAcceptNewOrders()
    {
        return IsDriver &&
               Status == EmployeeStatus.Active &&
               IsAvailable &&
               CurrentOrdersCount < MaxConcurrentOrders;
    }

    /// <summary>
    /// الحصول على اسم الدور بالعربية
    /// </summary>
    public string GetRoleDisplayName()
    {
        return Role switch
        {
            EmployeeRole.Driver => "سائق توصيل",
            EmployeeRole.Cashier => "كاشير",
            EmployeeRole.Manager => "مدير",
            EmployeeRole.Preparer => "محضر طلبات",
            _ => "موظف"
        };
    }

    /// <summary>
    /// الحصول على اسم الحالة بالعربية
    /// </summary>
    public string GetStatusDisplayName()
    {
        return Status switch
        {
            EmployeeStatus.Active => "نشط",
            EmployeeStatus.Inactive => "غير نشط",
            EmployeeStatus.Suspended => "معلق",
            _ => "غير معروف"
        };
    }

    /// <summary>
    /// الحصول على نوع المركبة بالعربية
    /// </summary>
    public string? GetVehicleTypeDisplayName()
    {
        return VehicleType switch
        {
            Enums.VehicleType.Motorcycle => "دراجة نارية",
            Enums.VehicleType.Car => "سيارة",
            Enums.VehicleType.Bicycle => "دراجة هوائية",
            _ => null
        };
    }
}

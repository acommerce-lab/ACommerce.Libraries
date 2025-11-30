using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Locations.Abstractions.Entities;

/// <summary>
/// المدينة
/// </summary>
public class City : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>
    /// الاسم (باللغة الافتراضية)
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// الاسم بالإنجليزية
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// الرمز
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// معرف المنطقة
    /// </summary>
    public Guid RegionId { get; set; }

    /// <summary>
    /// المنطقة
    /// </summary>
    public Region? Region { get; set; }

    /// <summary>
    /// خط العرض
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// خط الطول
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// الرمز البريدي (إذا موحد للمدينة)
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// عدد السكان (تقريبي)
    /// </summary>
    public int? Population { get; set; }

    /// <summary>
    /// المنطقة الزمنية (إذا مختلفة)
    /// </summary>
    public string? Timezone { get; set; }

    /// <summary>
    /// نشطة (متاحة للاستخدام)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// مدينة رئيسية (عاصمة المنطقة)
    /// </summary>
    public bool IsCapital { get; set; }

    /// <summary>
    /// ترتيب العرض
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// الأحياء
    /// </summary>
    public List<Neighborhood> Neighborhoods { get; set; } = [];
}

using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Locations.Abstractions.Entities;

/// <summary>
/// المنطقة / الإقليم / الولاية / المحافظة
/// </summary>
public class Region : IBaseEntity
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
    /// الرمز (مثل RUH للرياض)
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// نوع المنطقة
    /// </summary>
    public RegionType Type { get; set; } = RegionType.Region;

    /// <summary>
    /// معرف الدولة
    /// </summary>
    public Guid CountryId { get; set; }

    /// <summary>
    /// الدولة
    /// </summary>
    public Country? Country { get; set; }

    /// <summary>
    /// خط العرض (مركز المنطقة)
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// خط الطول (مركز المنطقة)
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// المنطقة الزمنية (إذا مختلفة عن الدولة)
    /// </summary>
    public string? Timezone { get; set; }

    /// <summary>
    /// نشطة (متاحة للاستخدام)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// ترتيب العرض
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// المدن
    /// </summary>
    public List<City> Cities { get; set; } = [];
}

/// <summary>
/// نوع المنطقة الإدارية
/// </summary>
public enum RegionType
{
    /// <summary>
    /// منطقة (مثل منطقة الرياض)
    /// </summary>
    Region = 1,

    /// <summary>
    /// إمارة
    /// </summary>
    Emirate = 2,

    /// <summary>
    /// محافظة
    /// </summary>
    Governorate = 3,

    /// <summary>
    /// ولاية
    /// </summary>
    State = 4,

    /// <summary>
    /// مقاطعة
    /// </summary>
    Province = 5
}

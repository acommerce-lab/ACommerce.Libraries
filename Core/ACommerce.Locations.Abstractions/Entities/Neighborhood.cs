using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Locations.Abstractions.Entities;

/// <summary>
/// الحي
/// </summary>
public class Neighborhood : IBaseEntity
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
    /// معرف المدينة
    /// </summary>
    public Guid CityId { get; set; }

    /// <summary>
    /// المدينة
    /// </summary>
    public City? City { get; set; }

    /// <summary>
    /// خط العرض (مركز الحي)
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// خط الطول (مركز الحي)
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// الرمز البريدي
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// حدود الحي (GeoJSON polygon) - للبحث الجغرافي
    /// </summary>
    public string? Boundaries { get; set; }

    /// <summary>
    /// نشط (متاح للاستخدام)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// ترتيب العرض
    /// </summary>
    public int SortOrder { get; set; }
}

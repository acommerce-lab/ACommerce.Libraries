using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Locations.Abstractions.Entities;

/// <summary>
/// الدولة
/// </summary>
public class Country : IBaseEntity
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
    /// رمز الدولة ISO 3166-1 alpha-2 (مثل SA, AE, EG)
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// رمز الدولة ISO 3166-1 alpha-3 (مثل SAU, ARE, EGY)
    /// </summary>
    public string? Code3 { get; set; }

    /// <summary>
    /// الرمز الرقمي ISO 3166-1 numeric
    /// </summary>
    public int? NumericCode { get; set; }

    /// <summary>
    /// رمز الاتصال الدولي (مثل +966)
    /// </summary>
    public string? PhoneCode { get; set; }

    /// <summary>
    /// رمز العملة (مثل SAR)
    /// </summary>
    public string? CurrencyCode { get; set; }

    /// <summary>
    /// اسم العملة
    /// </summary>
    public string? CurrencyName { get; set; }

    /// <summary>
    /// رمز العملة (مثل ر.س)
    /// </summary>
    public string? CurrencySymbol { get; set; }

    /// <summary>
    /// علم الدولة (emoji أو رابط)
    /// </summary>
    public string? Flag { get; set; }

    /// <summary>
    /// خط العرض (مركز الدولة)
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// خط الطول (مركز الدولة)
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// المنطقة الزمنية الافتراضية
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
    /// المناطق/الأقاليم
    /// </summary>
    public List<Region> Regions { get; set; } = [];
}

using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.Spaces.Enums;

namespace ACommerce.Spaces.Entities;

/// <summary>
/// تسعير المساحة
/// </summary>
public class SpacePrice : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>
    /// معرف المساحة
    /// </summary>
    public Guid SpaceId { get; set; }
    public Space? Space { get; set; }

    /// <summary>
    /// نوع التسعير
    /// </summary>
    public PricingType PricingType { get; set; }

    /// <summary>
    /// السعر
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// كود العملة
    /// </summary>
    public string CurrencyCode { get; set; } = "SAR";

    /// <summary>
    /// السعر قبل الخصم
    /// </summary>
    public decimal? OriginalPrice { get; set; }

    /// <summary>
    /// نسبة الخصم
    /// </summary>
    public decimal? DiscountPercentage { get; set; }

    /// <summary>
    /// تاريخ بداية العرض
    /// </summary>
    public DateTime? OfferStartDate { get; set; }

    /// <summary>
    /// تاريخ نهاية العرض
    /// </summary>
    public DateTime? OfferEndDate { get; set; }

    /// <summary>
    /// سعر نهاية الأسبوع
    /// </summary>
    public decimal? WeekendPrice { get; set; }

    /// <summary>
    /// سعر أيام العطل
    /// </summary>
    public decimal? HolidayPrice { get; set; }

    /// <summary>
    /// الحد الأدنى للحجز (للنوع المحدد)
    /// </summary>
    public int? MinimumUnits { get; set; }

    /// <summary>
    /// الحد الأقصى للحجز
    /// </summary>
    public int? MaximumUnits { get; set; }

    /// <summary>
    /// سعر فعال
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// السعر الافتراضي
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// ملاحظات
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// ترتيب العرض
    /// </summary>
    public int SortOrder { get; set; }
}

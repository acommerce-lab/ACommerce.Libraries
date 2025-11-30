using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Spaces.Entities;

/// <summary>
/// مرافق وخدمات المساحة
/// </summary>
public class SpaceAmenity : IBaseEntity
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
    /// اسم المرفق
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// الاسم بالإنجليزية
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// الفئة
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// الأيقونة
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// الوصف
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// مجاني
    /// </summary>
    public bool IsFree { get; set; } = true;

    /// <summary>
    /// السعر الإضافي
    /// </summary>
    public decimal? ExtraPrice { get; set; }

    /// <summary>
    /// نوع التسعير للمرفق
    /// </summary>
    public string? PricingUnit { get; set; }

    /// <summary>
    /// متاح
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// ملاحظات
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// ترتيب العرض
    /// </summary>
    public int SortOrder { get; set; }
}

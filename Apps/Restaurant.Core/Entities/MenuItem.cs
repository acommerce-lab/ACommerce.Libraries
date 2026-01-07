using ACommerce.SharedKernel.Abstractions.Entities;

namespace Restaurant.Core.Entities;

/// <summary>
/// عنصر القائمة (طبق/وجبة)
/// </summary>
public class MenuItem : IBaseEntity, IAuditableEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>
    /// معرف المطعم
    /// </summary>
    public Guid RestaurantProfileId { get; set; }
    public RestaurantProfile? Restaurant { get; set; }

    /// <summary>
    /// اسم الطبق
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// اسم الطبق بالإنجليزية
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// الوصف
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// الوصف بالإنجليزية
    /// </summary>
    public string? DescriptionEn { get; set; }

    /// <summary>
    /// السعر الأساسي
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// سعر البيع (إذا كان هناك عرض)
    /// </summary>
    public decimal? SalePrice { get; set; }

    /// <summary>
    /// صورة الطبق
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// معرف التصنيف
    /// </summary>
    public Guid? CategoryId { get; set; }
    public MenuCategory? Category { get; set; }

    /// <summary>
    /// وقت التحضير بالدقائق
    /// </summary>
    public int PreparationTimeMinutes { get; set; } = 15;

    /// <summary>
    /// السعرات الحرارية
    /// </summary>
    public int? Calories { get; set; }

    /// <summary>
    /// هل متاح حالياً
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// هل مميز
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// هل جديد
    /// </summary>
    public bool IsNew { get; set; }

    /// <summary>
    /// هل حار
    /// </summary>
    public bool IsSpicy { get; set; }

    /// <summary>
    /// هل نباتي
    /// </summary>
    public bool IsVegetarian { get; set; }

    /// <summary>
    /// هل خالي من الجلوتين
    /// </summary>
    public bool IsGlutenFree { get; set; }

    /// <summary>
    /// ترتيب العرض
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// المكونات
    /// </summary>
    public List<string> Ingredients { get; set; } = new();

    /// <summary>
    /// مسببات الحساسية
    /// </summary>
    public List<string> Allergens { get; set; } = new();

    /// <summary>
    /// الإضافات المتاحة
    /// </summary>
    public List<MenuItemAddon> Addons { get; set; } = new();

    /// <summary>
    /// الخيارات (حجم، نوع، إلخ)
    /// </summary>
    public List<MenuItemOption> Options { get; set; } = new();

    /// <summary>
    /// الحصول على السعر النهائي
    /// </summary>
    public decimal GetEffectivePrice()
    {
        if (SalePrice.HasValue && SalePrice.Value > 0 && SalePrice.Value < Price)
            return SalePrice.Value;
        return Price;
    }
}

/// <summary>
/// تصنيف القائمة
/// </summary>
public class MenuCategory : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>
    /// معرف المطعم
    /// </summary>
    public Guid RestaurantProfileId { get; set; }
    public RestaurantProfile? Restaurant { get; set; }

    /// <summary>
    /// اسم التصنيف
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// اسم التصنيف بالإنجليزية
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// الوصف
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// صورة التصنيف
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// ترتيب العرض
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// هل نشط
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// عناصر القائمة في هذا التصنيف
    /// </summary>
    public List<MenuItem> Items { get; set; } = new();
}

/// <summary>
/// إضافة للطبق
/// </summary>
public class MenuItemAddon : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>
    /// معرف عنصر القائمة
    /// </summary>
    public Guid MenuItemId { get; set; }
    public MenuItem? MenuItem { get; set; }

    /// <summary>
    /// اسم الإضافة
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// اسم الإضافة بالإنجليزية
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// السعر الإضافي
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// هل متاحة
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// الحد الأقصى للكمية
    /// </summary>
    public int MaxQuantity { get; set; } = 5;
}

/// <summary>
/// خيار للطبق (مثل الحجم، نوع الخبز، إلخ)
/// </summary>
public class MenuItemOption : IBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>
    /// معرف عنصر القائمة
    /// </summary>
    public Guid MenuItemId { get; set; }
    public MenuItem? MenuItem { get; set; }

    /// <summary>
    /// اسم مجموعة الخيارات (مثل: الحجم)
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// اسم مجموعة الخيارات بالإنجليزية
    /// </summary>
    public string? GroupNameEn { get; set; }

    /// <summary>
    /// هل الاختيار إجباري
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// هل يمكن اختيار أكثر من خيار
    /// </summary>
    public bool AllowMultiple { get; set; }

    /// <summary>
    /// الخيارات المتاحة
    /// </summary>
    public List<MenuItemOptionValue> Values { get; set; } = new();
}

/// <summary>
/// قيمة خيار
/// </summary>
public class MenuItemOptionValue
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// اسم الخيار (مثل: صغير، متوسط، كبير)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// اسم الخيار بالإنجليزية
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// فرق السعر (يمكن أن يكون سالب أو موجب)
    /// </summary>
    public decimal PriceDifference { get; set; }

    /// <summary>
    /// هل متاح
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// هل الافتراضي
    /// </summary>
    public bool IsDefault { get; set; }
}

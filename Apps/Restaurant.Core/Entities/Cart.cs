using ACommerce.Contracts.Interfaces;

namespace Restaurant.Core.Entities;

/// <summary>
/// سلة التسوق - لكل عميل سلة منفصلة لكل مطعم
/// </summary>
public class Cart : IBaseEntity, IAuditableEntity
{
    public Guid Id { get; set; }

    /// <summary>
    /// معرف العميل
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// معرف المطعم
    /// </summary>
    public Guid RestaurantId { get; set; }

    /// <summary>
    /// اسم المطعم (للعرض)
    /// </summary>
    public string RestaurantName { get; set; } = string.Empty;

    /// <summary>
    /// صورة المطعم
    /// </summary>
    public string? RestaurantImageUrl { get; set; }

    /// <summary>
    /// عناصر السلة
    /// </summary>
    public List<CartItem> Items { get; set; } = new();

    /// <summary>
    /// ملاحظات عامة على الطلب
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// المجموع الفرعي (قبل التوصيل)
    /// </summary>
    public decimal Subtotal => Items.Sum(i => i.TotalPrice);

    /// <summary>
    /// رسوم التوصيل
    /// </summary>
    public decimal DeliveryFee { get; set; }

    /// <summary>
    /// المجموع الكلي
    /// </summary>
    public decimal Total => Subtotal + DeliveryFee;

    /// <summary>
    /// عدد العناصر
    /// </summary>
    public int ItemsCount => Items.Sum(i => i.Quantity);

    /// <summary>
    /// الحد الأدنى للطلب
    /// </summary>
    public decimal MinimumOrderAmount { get; set; }

    /// <summary>
    /// هل تجاوز الحد الأدنى؟
    /// </summary>
    public bool MeetsMinimumOrder => Subtotal >= MinimumOrderAmount;

    /// <summary>
    /// تاريخ الإنشاء
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// تاريخ التحديث
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// المنشئ
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// المحدث
    /// </summary>
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// عنصر في السلة
/// </summary>
public class CartItem : IBaseEntity
{
    public Guid Id { get; set; }

    /// <summary>
    /// معرف السلة
    /// </summary>
    public Guid CartId { get; set; }

    /// <summary>
    /// معرف عنصر القائمة
    /// </summary>
    public Guid MenuItemId { get; set; }

    /// <summary>
    /// اسم العنصر
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// اسم العنصر بالإنجليزية
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// صورة العنصر
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// السعر الأساسي
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// الكمية
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// ملاحظات خاصة بالعنصر
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// الإضافات المختارة
    /// </summary>
    public List<CartItemAddon> SelectedAddons { get; set; } = new();

    /// <summary>
    /// الخيارات المختارة
    /// </summary>
    public List<CartItemOption> SelectedOptions { get; set; } = new();

    /// <summary>
    /// سعر الإضافات
    /// </summary>
    public decimal AddonsPrice => SelectedAddons.Sum(a => a.Price * a.Quantity);

    /// <summary>
    /// سعر الخيارات (الفرق في السعر)
    /// </summary>
    public decimal OptionsPrice => SelectedOptions.Sum(o => o.PriceDifference);

    /// <summary>
    /// السعر الكلي للعنصر
    /// </summary>
    public decimal TotalPrice => (UnitPrice + AddonsPrice + OptionsPrice) * Quantity;
}

/// <summary>
/// إضافة مختارة في عنصر السلة
/// </summary>
public class CartItemAddon
{
    /// <summary>
    /// معرف الإضافة الأصلية
    /// </summary>
    public Guid AddonId { get; set; }

    /// <summary>
    /// اسم الإضافة
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// السعر
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// الكمية
    /// </summary>
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// خيار مختار في عنصر السلة
/// </summary>
public class CartItemOption
{
    /// <summary>
    /// معرف مجموعة الخيارات
    /// </summary>
    public Guid OptionGroupId { get; set; }

    /// <summary>
    /// اسم مجموعة الخيارات
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// معرف القيمة المختارة
    /// </summary>
    public Guid OptionValueId { get; set; }

    /// <summary>
    /// اسم القيمة المختارة
    /// </summary>
    public string ValueName { get; set; } = string.Empty;

    /// <summary>
    /// فرق السعر
    /// </summary>
    public decimal PriceDifference { get; set; }
}

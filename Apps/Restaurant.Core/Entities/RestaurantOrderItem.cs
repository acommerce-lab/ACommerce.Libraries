using ACommerce.SharedKernel.Abstractions.Entities;

namespace Restaurant.Core.Entities;

/// <summary>
/// عنصر من عناصر الطلب (وجبة أو منتج)
/// </summary>
public class RestaurantOrderItem : IBaseEntity
{
    public Guid Id { get; set; }

    /// <summary>
    /// معرف الطلب
    /// </summary>
    public Guid RestaurantOrderId { get; set; }

    /// <summary>
    /// الطلب
    /// </summary>
    public RestaurantOrder? RestaurantOrder { get; set; }

    /// <summary>
    /// معرف العرض من ProductListing
    /// </summary>
    public Guid ListingId { get; set; }

    /// <summary>
    /// معرف المنتج الأصلي
    /// </summary>
    public Guid ProductId { get; set; }

    // === معلومات المنتج (نسخة عند وقت الطلب) ===

    /// <summary>
    /// اسم المنتج
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// وصف المنتج
    /// </summary>
    public string? ProductDescription { get; set; }

    /// <summary>
    /// صورة المنتج
    /// </summary>
    public string? ProductImageUrl { get; set; }

    // === الكمية والسعر ===

    /// <summary>
    /// الكمية
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// سعر الوحدة
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// المجموع (الكمية × السعر)
    /// </summary>
    public decimal Total { get; set; }

    // === الخيارات والإضافات ===

    /// <summary>
    /// الخيارات المحددة (مثل: الحجم، نوع الخبز)
    /// JSON: [{"name": "الحجم", "value": "كبير", "price": 5}]
    /// </summary>
    public string? SelectedOptions { get; set; }

    /// <summary>
    /// الإضافات المحددة (مثل: جبنة إضافية)
    /// JSON: [{"name": "جبنة إضافية", "price": 3}]
    /// </summary>
    public string? SelectedAddons { get; set; }

    /// <summary>
    /// ملاحظات خاصة بهذا العنصر (مثل: بدون بصل)
    /// </summary>
    public string? SpecialInstructions { get; set; }

    /// <summary>
    /// سعر الخيارات والإضافات الإضافي
    /// </summary>
    public decimal ExtrasPrice { get; set; } = 0;

    // === الدوال المساعدة ===

    /// <summary>
    /// حساب المجموع الكلي مع الإضافات
    /// </summary>
    public decimal CalculateTotal()
    {
        return (UnitPrice + ExtrasPrice) * Quantity;
    }

    /// <summary>
    /// الحصول على وصف مختصر للعنصر
    /// </summary>
    public string GetShortDescription()
    {
        var desc = $"{Quantity}x {ProductName}";
        if (!string.IsNullOrEmpty(SpecialInstructions))
        {
            desc += $" ({SpecialInstructions})";
        }
        return desc;
    }
}

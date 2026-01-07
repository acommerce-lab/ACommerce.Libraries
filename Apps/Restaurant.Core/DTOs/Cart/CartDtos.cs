namespace Restaurant.Core.DTOs.Cart;

/// <summary>
/// بيانات السلة
/// </summary>
public class CartDto
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public string? RestaurantImageUrl { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public string? Notes { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Total { get; set; }
    public int ItemsCount { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public bool MeetsMinimumOrder { get; set; }
    public decimal AmountToMinimum => MeetsMinimumOrder ? 0 : MinimumOrderAmount - Subtotal;
}

/// <summary>
/// عنصر في السلة
/// </summary>
public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
    public List<CartItemAddonDto> SelectedAddons { get; set; } = new();
    public List<CartItemOptionDto> SelectedOptions { get; set; } = new();
    public decimal AddonsPrice { get; set; }
    public decimal OptionsPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

/// <summary>
/// إضافة في عنصر السلة
/// </summary>
public class CartItemAddonDto
{
    public Guid AddonId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// خيار في عنصر السلة
/// </summary>
public class CartItemOptionDto
{
    public Guid OptionGroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public Guid OptionValueId { get; set; }
    public string ValueName { get; set; } = string.Empty;
    public decimal PriceDifference { get; set; }
}

/// <summary>
/// طلب إضافة عنصر للسلة
/// </summary>
public class AddToCartRequest
{
    /// <summary>
    /// معرف المطعم
    /// </summary>
    public Guid RestaurantId { get; set; }

    /// <summary>
    /// معرف عنصر القائمة
    /// </summary>
    public Guid MenuItemId { get; set; }

    /// <summary>
    /// الكمية
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// ملاحظات
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// الإضافات المختارة
    /// </summary>
    public List<SelectedAddonRequest>? Addons { get; set; }

    /// <summary>
    /// الخيارات المختارة
    /// </summary>
    public List<SelectedOptionRequest>? Options { get; set; }
}

/// <summary>
/// إضافة مختارة
/// </summary>
public class SelectedAddonRequest
{
    public Guid AddonId { get; set; }
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// خيار مختار
/// </summary>
public class SelectedOptionRequest
{
    public Guid OptionGroupId { get; set; }
    public Guid OptionValueId { get; set; }
}

/// <summary>
/// طلب تحديث كمية عنصر
/// </summary>
public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// طلب تحديث ملاحظات السلة
/// </summary>
public class UpdateCartNotesRequest
{
    public string? Notes { get; set; }
}

/// <summary>
/// ملخص جميع سلات العميل
/// </summary>
public class CartsSummaryDto
{
    /// <summary>
    /// عدد السلات النشطة
    /// </summary>
    public int ActiveCartsCount { get; set; }

    /// <summary>
    /// إجمالي العناصر في جميع السلات
    /// </summary>
    public int TotalItemsCount { get; set; }

    /// <summary>
    /// قائمة السلات
    /// </summary>
    public List<CartSummaryItemDto> Carts { get; set; } = new();
}

/// <summary>
/// ملخص سلة واحدة
/// </summary>
public class CartSummaryItemDto
{
    public Guid CartId { get; set; }
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public string? RestaurantImageUrl { get; set; }
    public int ItemsCount { get; set; }
    public decimal Total { get; set; }
}

/// <summary>
/// معلومات رسوم التوصيل
/// </summary>
public class DeliveryFeeInfoDto
{
    /// <summary>
    /// رسوم التوصيل
    /// </summary>
    public decimal Fee { get; set; }

    /// <summary>
    /// المسافة بالكيلومتر
    /// </summary>
    public double DistanceKm { get; set; }

    /// <summary>
    /// اسم المنطقة
    /// </summary>
    public string ZoneName { get; set; } = string.Empty;

    /// <summary>
    /// هل التوصيل متاح؟
    /// </summary>
    public bool IsDeliveryAvailable { get; set; }

    /// <summary>
    /// رسالة في حال عدم التوفر
    /// </summary>
    public string? UnavailableMessage { get; set; }

    /// <summary>
    /// الوقت المتوقع للتوصيل بالدقائق
    /// </summary>
    public int EstimatedMinutes { get; set; }
}

namespace Restaurant.Core.DTOs.Menu;

/// <summary>
/// عنصر القائمة للعرض
/// </summary>
public class MenuItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal EffectivePrice { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int PreparationTimeMinutes { get; set; }
    public int? Calories { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNew { get; set; }
    public bool IsSpicy { get; set; }
    public bool IsVegetarian { get; set; }
    public bool IsGlutenFree { get; set; }
    public List<string> Ingredients { get; set; } = new();
    public List<string> Allergens { get; set; } = new();
    public List<MenuItemAddonDto> Addons { get; set; } = new();
    public List<MenuItemOptionDto> Options { get; set; } = new();
}

/// <summary>
/// إضافة للطبق
/// </summary>
public class MenuItemAddonDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public int MaxQuantity { get; set; }
}

/// <summary>
/// خيار للطبق
/// </summary>
public class MenuItemOptionDto
{
    public Guid Id { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string? GroupNameEn { get; set; }
    public bool IsRequired { get; set; }
    public bool AllowMultiple { get; set; }
    public List<MenuItemOptionValueDto> Values { get; set; } = new();
}

/// <summary>
/// قيمة خيار
/// </summary>
public class MenuItemOptionValueDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public decimal PriceDifference { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsDefault { get; set; }
}

/// <summary>
/// تصنيف القائمة للعرض
/// </summary>
public class MenuCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int ItemsCount { get; set; }
    public List<MenuItemDto> Items { get; set; } = new();
}

/// <summary>
/// القائمة الكاملة للمطعم
/// </summary>
public class RestaurantMenuDto
{
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public List<MenuCategoryDto> Categories { get; set; } = new();
    public List<MenuItemDto> FeaturedItems { get; set; } = new();
    public int TotalItemsCount { get; set; }
}

/// <summary>
/// طلب إنشاء عنصر قائمة
/// </summary>
public class CreateMenuItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? CategoryId { get; set; }
    public int PreparationTimeMinutes { get; set; } = 15;
    public int? Calories { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsFeatured { get; set; }
    public bool IsNew { get; set; }
    public bool IsSpicy { get; set; }
    public bool IsVegetarian { get; set; }
    public bool IsGlutenFree { get; set; }
    public List<string> Ingredients { get; set; } = new();
    public List<string> Allergens { get; set; } = new();
}

/// <summary>
/// طلب تحديث عنصر قائمة
/// </summary>
public class UpdateMenuItemRequest : CreateMenuItemRequest
{
    public Guid Id { get; set; }
}

/// <summary>
/// طلب إنشاء تصنيف
/// </summary>
public class CreateMenuCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// طلب تحديث تصنيف
/// </summary>
public class UpdateMenuCategoryRequest : CreateMenuCategoryRequest
{
    public Guid Id { get; set; }
}

using Microsoft.AspNetCore.Mvc;
using Restaurant.Core.DTOs.Menu;

namespace Restaurant.Customer.Api.Controllers;

/// <summary>
/// متحكم القائمة (للعميل)
/// </summary>
[ApiController]
[Route("api/restaurants/{restaurantId}/[controller]")]
public class MenuController : ControllerBase
{
    /// <summary>
    /// الحصول على قائمة المطعم الكاملة
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<RestaurantMenuDto>> GetMenu(Guid restaurantId)
    {
        // TODO: جلب من قاعدة البيانات
        var menu = new RestaurantMenuDto
        {
            RestaurantId = restaurantId,
            RestaurantName = "مطعم البركة",
            TotalItemsCount = 15,
            Categories = new List<MenuCategoryDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "الأطباق الرئيسية",
                    NameEn = "Main Dishes",
                    SortOrder = 1,
                    IsActive = true,
                    ItemsCount = 8,
                    Items = GetSampleItems()
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "المقبلات",
                    NameEn = "Appetizers",
                    SortOrder = 2,
                    IsActive = true,
                    ItemsCount = 5
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "المشروبات",
                    NameEn = "Beverages",
                    SortOrder = 3,
                    IsActive = true,
                    ItemsCount = 2
                }
            },
            FeaturedItems = GetSampleItems().Where(i => i.IsFeatured).ToList()
        };

        return Ok(menu);
    }

    /// <summary>
    /// الحصول على عنصر محدد من القائمة
    /// </summary>
    [HttpGet("{itemId}")]
    public async Task<ActionResult<MenuItemDto>> GetMenuItem(Guid restaurantId, Guid itemId)
    {
        // TODO: جلب من قاعدة البيانات
        var item = GetSampleItems().FirstOrDefault();
        if (item == null)
            return NotFound(new { message = "العنصر غير موجود" });

        item.Id = itemId;
        return Ok(item);
    }

    /// <summary>
    /// الحصول على تصنيفات القائمة
    /// </summary>
    [HttpGet("categories")]
    public async Task<ActionResult<List<MenuCategoryDto>>> GetCategories(Guid restaurantId)
    {
        // TODO: جلب من قاعدة البيانات
        var categories = new List<MenuCategoryDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "الأطباق الرئيسية",
                NameEn = "Main Dishes",
                SortOrder = 1,
                IsActive = true,
                ItemsCount = 8
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "المقبلات",
                NameEn = "Appetizers",
                SortOrder = 2,
                IsActive = true,
                ItemsCount = 5
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "المشروبات",
                NameEn = "Beverages",
                SortOrder = 3,
                IsActive = true,
                ItemsCount = 2
            }
        };

        return Ok(categories);
    }

    /// <summary>
    /// الحصول على عناصر تصنيف معين
    /// </summary>
    [HttpGet("categories/{categoryId}/items")]
    public async Task<ActionResult<List<MenuItemDto>>> GetCategoryItems(Guid restaurantId, Guid categoryId)
    {
        // TODO: جلب من قاعدة البيانات
        var items = GetSampleItems();
        return Ok(items);
    }

    /// <summary>
    /// البحث في القائمة
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<List<MenuItemDto>>> SearchMenu(
        Guid restaurantId,
        [FromQuery] string? q,
        [FromQuery] bool? isVegetarian,
        [FromQuery] bool? isSpicy,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice)
    {
        // TODO: البحث في قاعدة البيانات
        var items = GetSampleItems();

        if (!string.IsNullOrWhiteSpace(q))
        {
            items = items.Where(i =>
                i.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (i.Description?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }

        if (isVegetarian.HasValue)
            items = items.Where(i => i.IsVegetarian == isVegetarian.Value).ToList();

        if (isSpicy.HasValue)
            items = items.Where(i => i.IsSpicy == isSpicy.Value).ToList();

        if (minPrice.HasValue)
            items = items.Where(i => i.EffectivePrice >= minPrice.Value).ToList();

        if (maxPrice.HasValue)
            items = items.Where(i => i.EffectivePrice <= maxPrice.Value).ToList();

        return Ok(items);
    }

    private List<MenuItemDto> GetSampleItems()
    {
        return new List<MenuItemDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "كبسة لحم",
                NameEn = "Lamb Kabsa",
                Description = "أرز بالبهارات العربية مع لحم ضأن طازج",
                Price = 45,
                EffectivePrice = 45,
                PreparationTimeMinutes = 20,
                Calories = 650,
                IsAvailable = true,
                IsFeatured = true,
                IsSpicy = false,
                Addons = new List<MenuItemAddonDto>
                {
                    new() { Id = Guid.NewGuid(), Name = "لحم إضافي", Price = 15, IsAvailable = true, MaxQuantity = 2 },
                    new() { Id = Guid.NewGuid(), Name = "رز إضافي", Price = 8, IsAvailable = true, MaxQuantity = 1 }
                },
                Options = new List<MenuItemOptionDto>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        GroupName = "الحجم",
                        GroupNameEn = "Size",
                        IsRequired = true,
                        AllowMultiple = false,
                        Values = new List<MenuItemOptionValueDto>
                        {
                            new() { Id = Guid.NewGuid(), Name = "فردي", PriceDifference = 0, IsAvailable = true, IsDefault = true },
                            new() { Id = Guid.NewGuid(), Name = "عائلي", PriceDifference = 30, IsAvailable = true }
                        }
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "مندي دجاج",
                NameEn = "Chicken Mandi",
                Description = "دجاج مدخن على الطريقة اليمنية مع أرز بسمتي",
                Price = 35,
                EffectivePrice = 35,
                PreparationTimeMinutes = 25,
                Calories = 550,
                IsAvailable = true,
                IsFeatured = true,
                IsSpicy = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "سلطة فتوش",
                NameEn = "Fattoush Salad",
                Description = "سلطة عربية طازجة مع خبز محمص",
                Price = 18,
                EffectivePrice = 18,
                PreparationTimeMinutes = 10,
                Calories = 180,
                IsAvailable = true,
                IsVegetarian = true,
                IsGlutenFree = false
            }
        };
    }
}

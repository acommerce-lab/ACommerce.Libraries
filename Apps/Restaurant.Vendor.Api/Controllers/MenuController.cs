using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Restaurant.Core.DTOs.Menu;

namespace Restaurant.Vendor.Api.Controllers;

/// <summary>
/// متحكم إدارة القائمة (لصاحب المطعم)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenuController : ControllerBase
{
    /// <summary>
    /// الحصول على قائمة المطعم
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<RestaurantMenuDto>> GetMenu()
    {
        // TODO: جلب RestaurantId من التوكن
        var restaurantId = Guid.NewGuid();

        var menu = new RestaurantMenuDto
        {
            RestaurantId = restaurantId,
            RestaurantName = "مطعمي",
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
                }
            }
        };

        return Ok(menu);
    }

    /// <summary>
    /// إضافة تصنيف جديد
    /// </summary>
    [HttpPost("categories")]
    public async Task<ActionResult<MenuCategoryDto>> CreateCategory([FromBody] CreateMenuCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "اسم التصنيف مطلوب" });

        var category = new MenuCategoryDto
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            NameEn = request.NameEn,
            Description = request.Description,
            SortOrder = request.SortOrder,
            IsActive = true,
            ItemsCount = 0
        };

        return CreatedAtAction(nameof(GetCategory), new { categoryId = category.Id }, category);
    }

    /// <summary>
    /// الحصول على تصنيف محدد
    /// </summary>
    [HttpGet("categories/{categoryId}")]
    public async Task<ActionResult<MenuCategoryDto>> GetCategory(Guid categoryId)
    {
        var category = new MenuCategoryDto
        {
            Id = categoryId,
            Name = "الأطباق الرئيسية",
            NameEn = "Main Dishes",
            SortOrder = 1,
            IsActive = true,
            ItemsCount = 5
        };

        return Ok(category);
    }

    /// <summary>
    /// تحديث تصنيف
    /// </summary>
    [HttpPut("categories/{categoryId}")]
    public async Task<ActionResult<MenuCategoryDto>> UpdateCategory(Guid categoryId, [FromBody] UpdateMenuCategoryRequest request)
    {
        var category = new MenuCategoryDto
        {
            Id = categoryId,
            Name = request.Name ?? "الأطباق الرئيسية",
            NameEn = request.NameEn,
            Description = request.Description,
            SortOrder = request.SortOrder ?? 1,
            IsActive = request.IsActive ?? true,
            ItemsCount = 5
        };

        return Ok(category);
    }

    /// <summary>
    /// حذف تصنيف
    /// </summary>
    [HttpDelete("categories/{categoryId}")]
    public async Task<ActionResult> DeleteCategory(Guid categoryId)
    {
        // TODO: التحقق من عدم وجود عناصر في التصنيف
        return Ok(new { message = "تم حذف التصنيف بنجاح" });
    }

    /// <summary>
    /// إضافة عنصر جديد للقائمة
    /// </summary>
    [HttpPost("items")]
    public async Task<ActionResult<MenuItemDto>> CreateItem([FromBody] CreateMenuItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "اسم العنصر مطلوب" });

        if (request.Price <= 0)
            return BadRequest(new { message = "السعر يجب أن يكون أكبر من صفر" });

        var item = new MenuItemDto
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            NameEn = request.NameEn,
            Description = request.Description,
            Price = request.Price,
            SalePrice = request.SalePrice,
            EffectivePrice = request.SalePrice ?? request.Price,
            ImageUrl = request.ImageUrl,
            PreparationTimeMinutes = request.PreparationTimeMinutes,
            Calories = request.Calories,
            IsAvailable = true,
            IsFeatured = request.IsFeatured,
            IsVegetarian = request.IsVegetarian,
            IsVegan = request.IsVegan,
            IsGlutenFree = request.IsGlutenFree,
            IsSpicy = request.IsSpicy
        };

        return CreatedAtAction(nameof(GetItem), new { itemId = item.Id }, item);
    }

    /// <summary>
    /// الحصول على عنصر محدد
    /// </summary>
    [HttpGet("items/{itemId}")]
    public async Task<ActionResult<MenuItemDto>> GetItem(Guid itemId)
    {
        var item = GetSampleItems().FirstOrDefault();
        if (item == null)
            return NotFound(new { message = "العنصر غير موجود" });

        item.Id = itemId;
        return Ok(item);
    }

    /// <summary>
    /// تحديث عنصر
    /// </summary>
    [HttpPut("items/{itemId}")]
    public async Task<ActionResult<MenuItemDto>> UpdateItem(Guid itemId, [FromBody] UpdateMenuItemRequest request)
    {
        var item = new MenuItemDto
        {
            Id = itemId,
            Name = request.Name ?? "عنصر",
            NameEn = request.NameEn,
            Description = request.Description,
            Price = request.Price ?? 0,
            SalePrice = request.SalePrice,
            EffectivePrice = request.SalePrice ?? request.Price ?? 0,
            ImageUrl = request.ImageUrl,
            PreparationTimeMinutes = request.PreparationTimeMinutes ?? 15,
            Calories = request.Calories,
            IsAvailable = request.IsAvailable ?? true,
            IsFeatured = request.IsFeatured ?? false,
            IsVegetarian = request.IsVegetarian ?? false,
            IsVegan = request.IsVegan ?? false,
            IsGlutenFree = request.IsGlutenFree ?? false,
            IsSpicy = request.IsSpicy ?? false
        };

        return Ok(item);
    }

    /// <summary>
    /// حذف عنصر
    /// </summary>
    [HttpDelete("items/{itemId}")]
    public async Task<ActionResult> DeleteItem(Guid itemId)
    {
        return Ok(new { message = "تم حذف العنصر بنجاح" });
    }

    /// <summary>
    /// تغيير توفر عنصر
    /// </summary>
    [HttpPatch("items/{itemId}/availability")]
    public async Task<ActionResult> ToggleItemAvailability(Guid itemId, [FromBody] bool isAvailable)
    {
        return Ok(new { message = isAvailable ? "تم تفعيل العنصر" : "تم إيقاف العنصر", isAvailable });
    }

    /// <summary>
    /// إضافة إضافة (addon) لعنصر
    /// </summary>
    [HttpPost("items/{itemId}/addons")]
    public async Task<ActionResult<MenuItemAddonDto>> AddItemAddon(Guid itemId, [FromBody] MenuItemAddonDto addon)
    {
        addon.Id = Guid.NewGuid();
        addon.IsAvailable = true;
        return CreatedAtAction(nameof(GetItem), new { itemId }, addon);
    }

    /// <summary>
    /// إضافة خيار لعنصر
    /// </summary>
    [HttpPost("items/{itemId}/options")]
    public async Task<ActionResult<MenuItemOptionDto>> AddItemOption(Guid itemId, [FromBody] MenuItemOptionDto option)
    {
        option.Id = Guid.NewGuid();
        return CreatedAtAction(nameof(GetItem), new { itemId }, option);
    }

    /// <summary>
    /// ترتيب التصنيفات
    /// </summary>
    [HttpPut("categories/reorder")]
    public async Task<ActionResult> ReorderCategories([FromBody] List<Guid> categoryIds)
    {
        return Ok(new { message = "تم تحديث الترتيب بنجاح" });
    }

    /// <summary>
    /// ترتيب العناصر في تصنيف
    /// </summary>
    [HttpPut("categories/{categoryId}/items/reorder")]
    public async Task<ActionResult> ReorderItems(Guid categoryId, [FromBody] List<Guid> itemIds)
    {
        return Ok(new { message = "تم تحديث ترتيب العناصر بنجاح" });
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
                IsFeatured = true
            }
        };
    }
}

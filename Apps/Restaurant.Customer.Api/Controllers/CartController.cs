using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Restaurant.Core.DTOs.Cart;

namespace Restaurant.Customer.Api.Controllers;

/// <summary>
/// متحكم السلة
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    /// <summary>
    /// الحصول على جميع سلات العميل
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<CartsSummaryDto>> GetAllCarts()
    {
        // TODO: جلب CustomerId من التوكن
        var summary = new CartsSummaryDto
        {
            ActiveCartsCount = 2,
            TotalItemsCount = 5,
            Carts = new List<CartSummaryItemDto>
            {
                new()
                {
                    CartId = Guid.NewGuid(),
                    RestaurantId = Guid.NewGuid(),
                    RestaurantName = "مطعم البركة",
                    ItemsCount = 3,
                    Total = 125
                },
                new()
                {
                    CartId = Guid.NewGuid(),
                    RestaurantId = Guid.NewGuid(),
                    RestaurantName = "مطعم الشرق",
                    ItemsCount = 2,
                    Total = 80
                }
            }
        };

        return Ok(summary);
    }

    /// <summary>
    /// الحصول على سلة مطعم محدد
    /// </summary>
    [HttpGet("restaurant/{restaurantId}")]
    public async Task<ActionResult<CartDto>> GetCartByRestaurant(Guid restaurantId)
    {
        var cart = GetSampleCart(restaurantId);
        return Ok(cart);
    }

    /// <summary>
    /// الحصول على سلة بالمعرف
    /// </summary>
    [HttpGet("{cartId}")]
    public async Task<ActionResult<CartDto>> GetCart(Guid cartId)
    {
        var cart = GetSampleCart(Guid.NewGuid());
        cart.Id = cartId;
        return Ok(cart);
    }

    /// <summary>
    /// إضافة عنصر للسلة
    /// </summary>
    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddToCart([FromBody] AddToCartRequest request)
    {
        if (request.Quantity <= 0)
            return BadRequest(new { message = "الكمية يجب أن تكون أكبر من صفر" });

        // TODO: التحقق من توفر المطعم وتوفر العنصر
        // TODO: إنشاء سلة جديدة إذا لم تكن موجودة

        var cart = GetSampleCart(request.RestaurantId);

        // إضافة العنصر الجديد
        cart.Items.Add(new CartItemDto
        {
            Id = Guid.NewGuid(),
            MenuItemId = request.MenuItemId,
            Name = "عنصر جديد",
            UnitPrice = 35,
            Quantity = request.Quantity,
            Notes = request.Notes,
            TotalPrice = 35 * request.Quantity
        });

        // إعادة حساب المجاميع
        cart.ItemsCount = cart.Items.Sum(i => i.Quantity);
        cart.Subtotal = cart.Items.Sum(i => i.TotalPrice);
        cart.Total = cart.Subtotal + cart.DeliveryFee;

        return Ok(cart);
    }

    /// <summary>
    /// تحديث كمية عنصر في السلة
    /// </summary>
    [HttpPut("items/{itemId}")]
    public async Task<ActionResult<CartDto>> UpdateCartItem(Guid itemId, [FromBody] UpdateCartItemRequest request)
    {
        if (request.Quantity <= 0)
            return BadRequest(new { message = "الكمية يجب أن تكون أكبر من صفر" });

        var cart = GetSampleCart(Guid.NewGuid());

        // تحديث العنصر
        var item = cart.Items.FirstOrDefault();
        if (item != null)
        {
            item.Id = itemId;
            item.Quantity = request.Quantity;
            item.Notes = request.Notes;
            item.TotalPrice = item.UnitPrice * request.Quantity;
        }

        return Ok(cart);
    }

    /// <summary>
    /// حذف عنصر من السلة
    /// </summary>
    [HttpDelete("items/{itemId}")]
    public async Task<ActionResult<CartDto>> RemoveFromCart(Guid itemId)
    {
        var cart = GetSampleCart(Guid.NewGuid());
        cart.Items = cart.Items.Where(i => i.Id != itemId).ToList();

        // إعادة حساب المجاميع
        cart.ItemsCount = cart.Items.Sum(i => i.Quantity);
        cart.Subtotal = cart.Items.Sum(i => i.TotalPrice);
        cart.Total = cart.Subtotal + cart.DeliveryFee;

        return Ok(cart);
    }

    /// <summary>
    /// تفريغ سلة مطعم
    /// </summary>
    [HttpDelete("restaurant/{restaurantId}")]
    public async Task<ActionResult> ClearCart(Guid restaurantId)
    {
        return Ok(new { message = "تم تفريغ السلة بنجاح" });
    }

    /// <summary>
    /// تحديث ملاحظات السلة
    /// </summary>
    [HttpPut("{cartId}/notes")]
    public async Task<ActionResult<CartDto>> UpdateCartNotes(Guid cartId, [FromBody] UpdateCartNotesRequest request)
    {
        var cart = GetSampleCart(Guid.NewGuid());
        cart.Id = cartId;
        cart.Notes = request.Notes;
        return Ok(cart);
    }

    /// <summary>
    /// حساب رسوم التوصيل
    /// </summary>
    [HttpGet("restaurant/{restaurantId}/delivery-fee")]
    public async Task<ActionResult<DeliveryFeeInfoDto>> CalculateDeliveryFee(
        Guid restaurantId,
        [FromQuery] double latitude,
        [FromQuery] double longitude)
    {
        // حساب المسافة (تقريبي)
        // TODO: استخدام Google Maps API للحساب الدقيق
        var distanceKm = CalculateDistance(latitude, longitude);

        var feeInfo = new DeliveryFeeInfoDto
        {
            DistanceKm = distanceKm,
            EstimatedMinutes = (int)(distanceKm * 4) + 15 // 4 دقائق لكل كم + وقت التحضير
        };

        // تحديد رسوم التوصيل حسب المنطقة
        if (distanceKm <= 3)
        {
            feeInfo.Fee = 0;
            feeInfo.ZoneName = "المنطقة القريبة (0-3 كم)";
            feeInfo.IsDeliveryAvailable = true;
        }
        else if (distanceKm <= 6)
        {
            feeInfo.Fee = 5;
            feeInfo.ZoneName = "المنطقة المتوسطة (3-6 كم)";
            feeInfo.IsDeliveryAvailable = true;
        }
        else if (distanceKm <= 10)
        {
            feeInfo.Fee = 10;
            feeInfo.ZoneName = "المنطقة البعيدة (6-10 كم)";
            feeInfo.IsDeliveryAvailable = true;
        }
        else
        {
            feeInfo.Fee = 0;
            feeInfo.ZoneName = "خارج نطاق التوصيل";
            feeInfo.IsDeliveryAvailable = false;
            feeInfo.UnavailableMessage = "عذراً، موقعك خارج نطاق التوصيل لهذا المطعم";
        }

        return Ok(feeInfo);
    }

    /// <summary>
    /// التحقق من صلاحية السلة للطلب
    /// </summary>
    [HttpGet("{cartId}/validate")]
    public async Task<ActionResult> ValidateCart(Guid cartId)
    {
        var cart = GetSampleCart(Guid.NewGuid());
        cart.Id = cartId;

        var errors = new List<string>();

        if (!cart.MeetsMinimumOrder)
            errors.Add($"الحد الأدنى للطلب {cart.MinimumOrderAmount} ر.س");

        if (cart.ItemsCount == 0)
            errors.Add("السلة فارغة");

        // TODO: التحقق من توفر جميع العناصر
        // TODO: التحقق من حالة المطعم

        if (errors.Any())
            return BadRequest(new { isValid = false, errors });

        return Ok(new { isValid = true, message = "السلة جاهزة للطلب" });
    }

    private CartDto GetSampleCart(Guid restaurantId)
    {
        return new CartDto
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            RestaurantName = "مطعم البركة",
            MinimumOrderAmount = 25,
            DeliveryFee = 5,
            Items = new List<CartItemDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    MenuItemId = Guid.NewGuid(),
                    Name = "كبسة لحم",
                    NameEn = "Lamb Kabsa",
                    UnitPrice = 45,
                    Quantity = 2,
                    TotalPrice = 90,
                    SelectedAddons = new List<CartItemAddonDto>
                    {
                        new() { AddonId = Guid.NewGuid(), Name = "لحم إضافي", Price = 15, Quantity = 1 }
                    },
                    AddonsPrice = 15,
                    SelectedOptions = new List<CartItemOptionDto>
                    {
                        new()
                        {
                            OptionGroupId = Guid.NewGuid(),
                            GroupName = "الحجم",
                            OptionValueId = Guid.NewGuid(),
                            ValueName = "عائلي",
                            PriceDifference = 30
                        }
                    },
                    OptionsPrice = 30
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    MenuItemId = Guid.NewGuid(),
                    Name = "سلطة فتوش",
                    NameEn = "Fattoush Salad",
                    UnitPrice = 18,
                    Quantity = 1,
                    TotalPrice = 18
                }
            },
            ItemsCount = 3,
            Subtotal = 153,
            Total = 158,
            MeetsMinimumOrder = true
        };
    }

    private double CalculateDistance(double lat, double lng)
    {
        // إحداثيات افتراضية للمطعم (الرياض)
        const double restaurantLat = 24.7136;
        const double restaurantLng = 46.6753;

        // حساب المسافة باستخدام صيغة Haversine
        const double R = 6371; // نصف قطر الأرض بالكيلومتر

        var dLat = ToRad(lat - restaurantLat);
        var dLon = ToRad(lng - restaurantLng);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(restaurantLat)) * Math.Cos(ToRad(lat)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private double ToRad(double deg) => deg * Math.PI / 180;
}

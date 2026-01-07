using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Restaurant.Core.DTOs.Order;
using Restaurant.Core.Enums;

namespace Restaurant.Customer.Api.Controllers;

/// <summary>
/// متحكم الطلبات (للعميل)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    /// <summary>
    /// إنشاء طلب جديد
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        // TODO: جلب السلة والتحقق منها
        // TODO: التحقق من حالة المطعم
        // TODO: إنشاء الطلب وإرسال إشعار للمطعم

        var order = new OrderDto
        {
            Id = Guid.NewGuid(),
            OrderNumber = $"ORD-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
            RestaurantId = request.RestaurantId,
            RestaurantName = "مطعم البركة",
            Status = OrderStatus.Pending,
            StatusText = "في انتظار قبول المطعم",
            Items = new List<OrderItemDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "كبسة لحم",
                    UnitPrice = 45,
                    Quantity = 2,
                    TotalPrice = 90
                }
            },
            Subtotal = 90,
            DeliveryFee = 5,
            Total = 95,
            CustomerNotes = request.Notes,
            DeliveryAddress = request.DeliveryAddress,
            EstimatedDeliveryMinutes = 45,
            CreatedAt = DateTime.UtcNow
        };

        return CreatedAtAction(nameof(GetOrder), new { orderId = order.Id }, order);
    }

    /// <summary>
    /// الحصول على طلب محدد
    /// </summary>
    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid orderId)
    {
        var order = GetSampleOrder();
        order.Id = orderId;
        return Ok(order);
    }

    /// <summary>
    /// الحصول على قائمة طلبات العميل
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<OrderSummaryDto>>> GetOrders(
        [FromQuery] OrderStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var orders = new List<OrderSummaryDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-20240115-1234",
                RestaurantName = "مطعم البركة",
                Status = OrderStatus.Delivered,
                StatusText = "تم التوصيل",
                ItemsCount = 3,
                Total = 125,
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new()
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-20240115-1235",
                RestaurantName = "مطعم الشرق",
                Status = OrderStatus.OnTheWay,
                StatusText = "في الطريق",
                ItemsCount = 2,
                Total = 80,
                CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                EstimatedMinutes = 15
            }
        };

        if (status.HasValue)
            orders = orders.Where(o => o.Status == status.Value).ToList();

        return Ok(orders);
    }

    /// <summary>
    /// تتبع الطلب
    /// </summary>
    [HttpGet("{orderId}/track")]
    public async Task<ActionResult<OrderTrackingDto>> TrackOrder(Guid orderId)
    {
        var tracking = new OrderTrackingDto
        {
            OrderId = orderId,
            OrderNumber = "ORD-20240115-1235",
            Status = OrderStatus.OnTheWay,
            StatusText = "في الطريق إليك",
            StatusMessage = "السائق أحمد في طريقه إليك",
            EstimatedMinutes = 12,
            Stages = new List<OrderStageDto>
            {
                new() { Name = "تم الطلب", Icon = "bi-check-circle", IsCompleted = true, CompletedAt = DateTime.UtcNow.AddMinutes(-45) },
                new() { Name = "قبول المطعم", Icon = "bi-shop", IsCompleted = true, CompletedAt = DateTime.UtcNow.AddMinutes(-40) },
                new() { Name = "جاري التحضير", Icon = "bi-fire", IsCompleted = true, CompletedAt = DateTime.UtcNow.AddMinutes(-20) },
                new() { Name = "استلام السائق", Icon = "bi-bicycle", IsCompleted = true, CompletedAt = DateTime.UtcNow.AddMinutes(-10) },
                new() { Name = "في الطريق", Icon = "bi-geo-alt", IsCompleted = false, IsCurrent = true },
                new() { Name = "تم التوصيل", Icon = "bi-house-check", IsCompleted = false }
            },
            DriverLocation = new DriverLocationDto
            {
                Latitude = 24.7200,
                Longitude = 46.6800,
                UpdatedAt = DateTime.UtcNow
            }
        };

        return Ok(tracking);
    }

    /// <summary>
    /// إلغاء الطلب
    /// </summary>
    [HttpPost("{orderId}/cancel")]
    public async Task<ActionResult> CancelOrder(Guid orderId, [FromBody] CancelOrderRequest request)
    {
        // TODO: التحقق من إمكانية الإلغاء (حسب الحالة)
        // TODO: إرسال إشعار للمطعم والسائق

        return Ok(new { message = "تم إلغاء الطلب بنجاح", reason = request.Reason });
    }

    /// <summary>
    /// إعادة الطلب (نسخ إلى السلة)
    /// </summary>
    [HttpPost("{orderId}/reorder")]
    public async Task<ActionResult> Reorder(Guid orderId)
    {
        // TODO: نسخ عناصر الطلب إلى سلة جديدة
        return Ok(new { message = "تم إضافة العناصر للسلة", cartId = Guid.NewGuid() });
    }

    private OrderDto GetSampleOrder()
    {
        return new OrderDto
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-20240115-1234",
            RestaurantId = Guid.NewGuid(),
            RestaurantName = "مطعم البركة",
            Status = OrderStatus.Preparing,
            StatusText = "جاري التحضير",
            Items = new List<OrderItemDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "كبسة لحم",
                    UnitPrice = 45,
                    Quantity = 2,
                    TotalPrice = 90,
                    Addons = new List<OrderItemAddonDto>
                    {
                        new() { Name = "لحم إضافي", Price = 15, Quantity = 1 }
                    }
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "سلطة فتوش",
                    UnitPrice = 18,
                    Quantity = 1,
                    TotalPrice = 18
                }
            },
            Subtotal = 123,
            DeliveryFee = 5,
            Total = 128,
            EstimatedDeliveryMinutes = 35,
            CreatedAt = DateTime.UtcNow.AddMinutes(-15)
        };
    }
}

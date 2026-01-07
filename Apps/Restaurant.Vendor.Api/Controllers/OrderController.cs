using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Restaurant.Core.DTOs.Order;
using Restaurant.Core.Enums;

namespace Restaurant.Vendor.Api.Controllers;

/// <summary>
/// متحكم الطلبات (لصاحب المطعم)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    /// <summary>
    /// الحصول على الطلبات الواردة (الرادار)
    /// </summary>
    [HttpGet("radar")]
    public async Task<ActionResult<List<RadarOrderDto>>> GetRadarOrders()
    {
        var orders = new List<RadarOrderDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-20240115-1234",
                CustomerName = "محمد أحمد",
                CustomerPhone = "+966501234567",
                Items = new List<OrderItemDto>
                {
                    new() { Name = "كبسة لحم", Quantity = 2, UnitPrice = 45, TotalPrice = 90 },
                    new() { Name = "سلطة فتوش", Quantity = 1, UnitPrice = 18, TotalPrice = 18 }
                },
                Total = 113,
                Notes = "بدون بصل",
                DeliveryAddress = new DeliveryAddressDto
                {
                    Label = "المنزل",
                    AddressLine = "حي الملقا، شارع الأمير سلطان",
                    BuildingNumber = "12",
                    Floor = "2",
                    Latitude = 24.7500,
                    Longitude = 46.6500
                },
                DistanceKm = 3.5,
                CreatedAt = DateTime.UtcNow.AddMinutes(-2),
                SecondsToAccept = 180 // 3 دقائق للقبول
            },
            new()
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-20240115-1235",
                CustomerName = "فاطمة علي",
                CustomerPhone = "+966509876543",
                Items = new List<OrderItemDto>
                {
                    new() { Name = "مندي دجاج", Quantity = 1, UnitPrice = 35, TotalPrice = 35 }
                },
                Total = 40,
                DeliveryAddress = new DeliveryAddressDto
                {
                    Label = "العمل",
                    AddressLine = "طريق الملك فهد",
                    Latitude = 24.7300,
                    Longitude = 46.6700
                },
                DistanceKm = 2.1,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1),
                SecondsToAccept = 240
            }
        };

        return Ok(orders);
    }

    /// <summary>
    /// قبول أو رفض طلب
    /// </summary>
    [HttpPost("{orderId}/respond")]
    public async Task<ActionResult<OrderDto>> RespondToOrder(Guid orderId, [FromBody] RestaurantOrderResponse response)
    {
        if (response.Accepted)
        {
            // TODO: تحديث حالة الطلب إلى مقبول
            // TODO: إرسال إشعار للعميل
            // TODO: تعيين سائق (إذا كان متاح)
            return Ok(new
            {
                message = "تم قبول الطلب",
                orderId,
                preparationMinutes = response.PreparationMinutes ?? 20,
                status = OrderStatus.Accepted
            });
        }
        else
        {
            // TODO: تحديث حالة الطلب إلى مرفوض
            // TODO: إرسال إشعار للعميل
            return Ok(new
            {
                message = "تم رفض الطلب",
                orderId,
                reason = response.RejectionReason,
                status = OrderStatus.Rejected
            });
        }
    }

    /// <summary>
    /// الحصول على الطلبات النشطة
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<List<OrderDto>>> GetActiveOrders()
    {
        var orders = new List<OrderDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-20240115-1230",
                CustomerName = "خالد سعد",
                Status = OrderStatus.Preparing,
                StatusText = "جاري التحضير",
                Items = new List<OrderItemDto>
                {
                    new() { Name = "كبسة لحم عائلي", Quantity = 1, TotalPrice = 75 }
                },
                Total = 80,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            },
            new()
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-20240115-1231",
                CustomerName = "سارة محمد",
                Status = OrderStatus.Ready,
                StatusText = "جاهز للاستلام",
                DriverName = "أحمد",
                Items = new List<OrderItemDto>
                {
                    new() { Name = "مندي دجاج", Quantity = 2, TotalPrice = 70 }
                },
                Total = 75,
                CreatedAt = DateTime.UtcNow.AddMinutes(-25)
            }
        };

        return Ok(orders);
    }

    /// <summary>
    /// الحصول على طلب محدد
    /// </summary>
    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid orderId)
    {
        var order = new OrderDto
        {
            Id = orderId,
            OrderNumber = "ORD-20240115-1234",
            CustomerName = "محمد أحمد",
            CustomerPhone = "+966501234567",
            Status = OrderStatus.Preparing,
            StatusText = "جاري التحضير",
            Items = new List<OrderItemDto>
            {
                new() { Name = "كبسة لحم", Quantity = 2, UnitPrice = 45, TotalPrice = 90 }
            },
            Subtotal = 90,
            DeliveryFee = 5,
            Total = 95,
            CreatedAt = DateTime.UtcNow.AddMinutes(-15)
        };

        return Ok(order);
    }

    /// <summary>
    /// تحديث حالة الطلب (جاهز للاستلام)
    /// </summary>
    [HttpPut("{orderId}/status")]
    public async Task<ActionResult> UpdateOrderStatus(Guid orderId, [FromBody] UpdateOrderStatusRequest request)
    {
        // TODO: التحقق من صلاحية تغيير الحالة
        // TODO: إرسال إشعار للعميل والسائق

        var statusText = request.NewStatus switch
        {
            OrderStatus.Preparing => "جاري التحضير",
            OrderStatus.Ready => "جاهز للاستلام",
            _ => "تم التحديث"
        };

        return Ok(new
        {
            message = $"تم تحديث حالة الطلب: {statusText}",
            orderId,
            newStatus = request.NewStatus
        });
    }

    /// <summary>
    /// تعيين سائق للطلب
    /// </summary>
    [HttpPost("{orderId}/assign-driver")]
    public async Task<ActionResult> AssignDriver(Guid orderId, [FromBody] Guid driverId)
    {
        // TODO: التحقق من أن السائق متاح
        // TODO: إرسال إشعار للسائق

        return Ok(new
        {
            message = "تم تعيين السائق",
            orderId,
            driverId
        });
    }

    /// <summary>
    /// الحصول على تاريخ الطلبات
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<List<OrderSummaryDto>>> GetOrderHistory(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] OrderStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var orders = new List<OrderSummaryDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-20240114-1200",
                Status = OrderStatus.Delivered,
                StatusText = "تم التوصيل",
                ItemsCount = 4,
                Total = 180,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-20240114-1150",
                Status = OrderStatus.Cancelled,
                StatusText = "ملغي",
                ItemsCount = 2,
                Total = 65,
                CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-2)
            }
        };

        return Ok(orders);
    }

    /// <summary>
    /// إحصائيات الطلبات
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult> GetOrderStats([FromQuery] DateTime? date)
    {
        var stats = new
        {
            today = new
            {
                totalOrders = 25,
                completedOrders = 20,
                cancelledOrders = 2,
                pendingOrders = 3,
                totalRevenue = 3250.00m,
                averageOrderValue = 130.00m
            },
            thisWeek = new
            {
                totalOrders = 150,
                completedOrders = 140,
                cancelledOrders = 10,
                totalRevenue = 19500.00m
            },
            averagePreparationTime = 18, // بالدقائق
            averageDeliveryTime = 32
        };

        return Ok(stats);
    }
}

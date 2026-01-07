using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Restaurant.Core.DTOs.Order;
using Restaurant.Core.Enums;

namespace Restaurant.Driver.Api.Controllers;

/// <summary>
/// متحكم الطلبات (للسائق)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    /// <summary>
    /// الحصول على الطلبات المتاحة للاستلام
    /// </summary>
    [HttpGet("available")]
    public async Task<ActionResult<List<DriverOrderDto>>> GetAvailableOrders()
    {
        // TODO: جلب الطلبات الجاهزة للمطعم الذي يعمل به السائق
        var orders = new List<DriverOrderDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-20240115-1234",
                Status = OrderStatus.Ready,
                StatusText = "جاهز للاستلام",
                RestaurantName = "مطعم البركة",
                RestaurantPhone = "+966112345678",
                RestaurantLatitude = 24.7136,
                RestaurantLongitude = 46.6753,
                RestaurantAddress = "حي العليا، شارع التحلية",
                CustomerName = "محمد أحمد",
                CustomerPhone = "+966501234567",
                CustomerAddress = new DeliveryAddressDto
                {
                    Label = "المنزل",
                    AddressLine = "حي الملقا، شارع الأمير سلطان",
                    BuildingNumber = "12",
                    Floor = "2",
                    Latitude = 24.7500,
                    Longitude = 46.6500
                },
                ItemsCount = 3,
                Total = 113,
                DeliveryFee = 5,
                DriverEarnings = 12,
                CreatedAt = DateTime.UtcNow.AddMinutes(-20),
                IsReadyForPickup = true
            }
        };

        return Ok(orders);
    }

    /// <summary>
    /// الحصول على الطلبات الحالية للسائق
    /// </summary>
    [HttpGet("current")]
    public async Task<ActionResult<List<DriverOrderDto>>> GetCurrentOrders()
    {
        // TODO: جلب الطلبات المسندة للسائق
        var orders = new List<DriverOrderDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-20240115-1230",
                Status = OrderStatus.OnTheWay,
                StatusText = "في الطريق للعميل",
                RestaurantName = "مطعم البركة",
                CustomerName = "خالد سعد",
                CustomerPhone = "+966507654321",
                CustomerAddress = new DeliveryAddressDto
                {
                    Label = "العمل",
                    AddressLine = "طريق الملك فهد، برج الفيصلية",
                    Latitude = 24.6900,
                    Longitude = 46.6850
                },
                ItemsCount = 2,
                Total = 80,
                DeliveryFee = 5,
                DriverEarnings = 10,
                PickedUpAt = DateTime.UtcNow.AddMinutes(-5)
            }
        };

        return Ok(orders);
    }

    /// <summary>
    /// الحصول على تفاصيل طلب
    /// </summary>
    [HttpGet("{orderId}")]
    public async Task<ActionResult<DriverOrderDto>> GetOrder(Guid orderId)
    {
        var order = new DriverOrderDto
        {
            Id = orderId,
            OrderNumber = "ORD-20240115-1234",
            Status = OrderStatus.Ready,
            StatusText = "جاهز للاستلام",
            RestaurantName = "مطعم البركة",
            RestaurantPhone = "+966112345678",
            RestaurantLatitude = 24.7136,
            RestaurantLongitude = 46.6753,
            RestaurantAddress = "حي العليا، شارع التحلية",
            CustomerName = "محمد أحمد",
            CustomerPhone = "+966501234567",
            CustomerAddress = new DeliveryAddressDto
            {
                Label = "المنزل",
                AddressLine = "حي الملقا، شارع الأمير سلطان",
                BuildingNumber = "12",
                Floor = "2",
                Apartment = "3",
                Landmark = "بجانب مسجد الراشد",
                Latitude = 24.7500,
                Longitude = 46.6500
            },
            ItemsCount = 3,
            Total = 113,
            DeliveryFee = 5,
            DriverEarnings = 12,
            CreatedAt = DateTime.UtcNow.AddMinutes(-20),
            IsReadyForPickup = true
        };

        return Ok(order);
    }

    /// <summary>
    /// قبول طلب
    /// </summary>
    [HttpPost("{orderId}/accept")]
    public async Task<ActionResult> AcceptOrder(Guid orderId)
    {
        // TODO: تعيين الطلب للسائق
        // TODO: إرسال إشعار للمطعم والعميل

        return Ok(new
        {
            message = "تم قبول الطلب",
            orderId,
            status = OrderStatus.PickingUp
        });
    }

    /// <summary>
    /// تأكيد استلام الطلب من المطعم
    /// </summary>
    [HttpPost("{orderId}/pickup")]
    public async Task<ActionResult> ConfirmPickup(Guid orderId, [FromBody] PickupConfirmationRequest? request)
    {
        // TODO: تحديث حالة الطلب
        // TODO: إنشاء مستند استلام
        // TODO: إرسال إشعار للعميل

        return Ok(new
        {
            message = "تم تأكيد استلام الطلب من المطعم",
            orderId,
            status = OrderStatus.OnTheWay,
            pickedUpAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// تأكيد التوصيل للعميل
    /// </summary>
    [HttpPost("{orderId}/deliver")]
    public async Task<ActionResult> ConfirmDelivery(Guid orderId, [FromBody] DeliveryConfirmationRequest? request)
    {
        // TODO: تحديث حالة الطلب
        // TODO: إنشاء مستند تسليم
        // TODO: إرسال إشعار للمطعم والعميل
        // TODO: إضافة الأرباح للسائق

        return Ok(new
        {
            message = "تم تأكيد توصيل الطلب",
            orderId,
            status = OrderStatus.Delivered,
            deliveredAt = DateTime.UtcNow,
            earnings = 12m
        });
    }

    /// <summary>
    /// تحديث موقع السائق
    /// </summary>
    [HttpPost("location")]
    public async Task<ActionResult> UpdateLocation([FromBody] UpdateLocationRequest request)
    {
        // TODO: تحديث الموقع في قاعدة البيانات
        // TODO: إرسال الموقع عبر SignalR للعميل

        return Ok(new
        {
            message = "تم تحديث الموقع",
            latitude = request.Latitude,
            longitude = request.Longitude,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// الإبلاغ عن مشكلة
    /// </summary>
    [HttpPost("{orderId}/report-issue")]
    public async Task<ActionResult> ReportIssue(Guid orderId, [FromBody] ReportIssueRequest request)
    {
        // TODO: تسجيل المشكلة
        // TODO: إرسال إشعار للمطعم

        return Ok(new
        {
            message = "تم تسجيل المشكلة",
            orderId,
            issueType = request.IssueType,
            ticketId = Guid.NewGuid()
        });
    }

    /// <summary>
    /// سجل التوصيلات
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<List<DriverOrderDto>>> GetDeliveryHistory(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var orders = new List<DriverOrderDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-20240114-1200",
                Status = OrderStatus.Delivered,
                StatusText = "تم التوصيل",
                RestaurantName = "مطعم البركة",
                CustomerName = "أحمد محمد",
                Total = 95,
                DriverEarnings = 10,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD-20240114-1150",
                Status = OrderStatus.Delivered,
                StatusText = "تم التوصيل",
                RestaurantName = "مطعم الشرق",
                CustomerName = "سارة علي",
                Total = 120,
                DriverEarnings = 15,
                CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-2)
            }
        };

        return Ok(orders);
    }

    /// <summary>
    /// إحصائيات الأرباح
    /// </summary>
    [HttpGet("earnings")]
    public async Task<ActionResult> GetEarnings([FromQuery] DateTime? date)
    {
        var earnings = new
        {
            today = new
            {
                deliveries = 8,
                totalEarnings = 96.00m,
                averagePerDelivery = 12.00m,
                totalDistance = 45.5 // كم
            },
            thisWeek = new
            {
                deliveries = 45,
                totalEarnings = 540.00m,
                averagePerDelivery = 12.00m,
                totalDistance = 280.0
            },
            thisMonth = new
            {
                deliveries = 180,
                totalEarnings = 2160.00m
            },
            rating = 4.8,
            totalDeliveries = 450
        };

        return Ok(earnings);
    }
}

/// <summary>
/// تأكيد الاستلام من المطعم
/// </summary>
public class PickupConfirmationRequest
{
    public string? Notes { get; set; }
}

/// <summary>
/// تأكيد التوصيل
/// </summary>
public class DeliveryConfirmationRequest
{
    public string? Notes { get; set; }
    public string? CustomerSignature { get; set; }
}

/// <summary>
/// تحديث الموقع
/// </summary>
public class UpdateLocationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Speed { get; set; }
    public double? Heading { get; set; }
}

/// <summary>
/// الإبلاغ عن مشكلة
/// </summary>
public class ReportIssueRequest
{
    public string IssueType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Restaurant.Core.Entities;
using Restaurant.Core.Enums;
using Restaurant.Driver.Api.Hubs;

namespace Restaurant.Driver.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly DriverDbContext _context;
    private readonly IHubContext<DriverOrdersHub> _hubContext;

    public OrdersController(DriverDbContext context, IHubContext<DriverOrdersHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    /// <summary>
    /// الحصول على الطلبات المعينة للسائق
    /// </summary>
    [HttpGet("assigned/{driverId}")]
    public async Task<ActionResult<List<object>>> GetAssignedOrders(Guid driverId)
    {
        var orders = await _context.RestaurantOrders
            .Include(o => o.Items)
            .Where(o => o.AssignedDriverId == driverId &&
                       (o.Status == RestaurantOrderStatus.AssignedToDriver ||
                        o.Status == RestaurantOrderStatus.DriverPickedUp ||
                        o.Status == RestaurantOrderStatus.OnTheWay))
            .OrderBy(o => o.ReadyAt)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                o.Barcode,
                Status = o.Status.ToString(),
                StatusDisplay = GetStatusDisplay(o.Status),

                // المطعم
                RestaurantName = "مطعم البرجر", // TODO: من Vendor
                RestaurantAddress = "الرياض، حي النخيل",

                // العميل
                o.CustomerName,
                o.CustomerPhone,
                o.DeliveryAddress,
                o.AddressDetails,
                o.DeliveryLatitude,
                o.DeliveryLongitude,

                // الطلب
                o.Total,
                ItemsCount = o.Items.Count,
                ItemsSummary = o.Items.Select(i => $"{i.Quantity}x {i.ProductName}").ToList(),

                // التوقيت
                o.ReadyAt
            })
            .ToListAsync();

        return Ok(orders);
    }

    /// <summary>
    /// استلام الطلب (مسح الباركود)
    /// </summary>
    [HttpPost("pickup/{orderId}")]
    public async Task<ActionResult> PickupOrder(Guid orderId, [FromBody] PickupRequest request)
    {
        var order = await _context.RestaurantOrders
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return NotFound(new { message = "الطلب غير موجود" });
        }

        // التحقق من الباركود
        if (!order.Barcode.Equals(request.Barcode, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "الباركود غير صحيح" });
        }

        if (order.Status != RestaurantOrderStatus.AssignedToDriver &&
            order.Status != RestaurantOrderStatus.Ready)
        {
            return BadRequest(new { message = "لا يمكن استلام هذا الطلب" });
        }

        var previousStatus = order.Status;
        order.Status = RestaurantOrderStatus.DriverPickedUp;
        order.PickedUpAt = DateTime.UtcNow;

        // تسجيل في جدول التعيينات
        var assignment = await _context.OrderDriverAssignments
            .FirstOrDefaultAsync(a => a.RestaurantOrderId == orderId && !a.IsCancelled);

        if (assignment != null)
        {
            assignment.RecordPickup(request.Barcode, request.Latitude, request.Longitude);
        }

        // إضافة سجل التغيير
        var history = OrderStatusHistory.Create(
            orderId,
            previousStatus,
            RestaurantOrderStatus.DriverPickedUp,
            OrderStatusChangedBy.Driver,
            latitude: request.Latitude,
            longitude: request.Longitude,
            notes: "تم استلام الطلب بمسح الباركود"
        );
        _context.OrderStatusHistories.Add(history);

        await _context.SaveChangesAsync();

        // إشعار العميل والمطعم
        await _hubContext.Clients
            .Group($"order-{orderId}")
            .SendAsync("OrderPickedUp", new
            {
                orderId,
                status = RestaurantOrderStatus.DriverPickedUp,
                message = "السائق استلم طلبك"
            });

        return Ok(new
        {
            message = "تم استلام الطلب بنجاح",
            orderId,
            status = RestaurantOrderStatus.DriverPickedUp
        });
    }

    /// <summary>
    /// بدء التوصيل (في الطريق)
    /// </summary>
    [HttpPost("on-way/{orderId}")]
    public async Task<ActionResult> StartDelivery(Guid orderId, [FromBody] LocationRequest request)
    {
        var order = await _context.RestaurantOrders
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return NotFound(new { message = "الطلب غير موجود" });
        }

        if (order.Status != RestaurantOrderStatus.DriverPickedUp)
        {
            return BadRequest(new { message = "يجب استلام الطلب أولاً" });
        }

        var previousStatus = order.Status;
        order.Status = RestaurantOrderStatus.OnTheWay;

        var history = OrderStatusHistory.Create(
            orderId,
            previousStatus,
            RestaurantOrderStatus.OnTheWay,
            OrderStatusChangedBy.Driver,
            latitude: request.Latitude,
            longitude: request.Longitude
        );
        _context.OrderStatusHistories.Add(history);

        await _context.SaveChangesAsync();

        await _hubContext.Clients
            .Group($"order-{orderId}")
            .SendAsync("OrderOnTheWay", new
            {
                orderId,
                status = RestaurantOrderStatus.OnTheWay,
                driverLatitude = request.Latitude,
                driverLongitude = request.Longitude,
                message = "السائق في الطريق إليك"
            });

        return Ok(new { message = "تم بدء التوصيل", status = RestaurantOrderStatus.OnTheWay });
    }

    /// <summary>
    /// تأكيد التسليم
    /// </summary>
    [HttpPost("delivered/{orderId}")]
    public async Task<ActionResult> ConfirmDelivery(Guid orderId, [FromBody] DeliveryConfirmationRequest request)
    {
        var order = await _context.RestaurantOrders
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return NotFound(new { message = "الطلب غير موجود" });
        }

        if (order.Status != RestaurantOrderStatus.OnTheWay)
        {
            return BadRequest(new { message = "الطلب ليس في الطريق" });
        }

        var previousStatus = order.Status;
        order.Status = RestaurantOrderStatus.Delivered;
        order.DeliveredAt = DateTime.UtcNow;

        // تحديث التعيين
        var assignment = await _context.OrderDriverAssignments
            .FirstOrDefaultAsync(a => a.RestaurantOrderId == orderId && !a.IsCancelled);

        if (assignment != null)
        {
            assignment.RecordDelivery(
                request.Latitude,
                request.Longitude,
                request.ProofImageUrl,
                request.Notes
            );
        }

        var history = OrderStatusHistory.Create(
            orderId,
            previousStatus,
            RestaurantOrderStatus.Delivered,
            OrderStatusChangedBy.Driver,
            latitude: request.Latitude,
            longitude: request.Longitude,
            notes: request.Notes
        );
        _context.OrderStatusHistories.Add(history);

        // تحديث إحصائيات السائق
        if (order.AssignedDriverId.HasValue)
        {
            var driver = await _context.VendorEmployees
                .FirstOrDefaultAsync(e => e.Id == order.AssignedDriverId.Value);

            if (driver != null)
            {
                driver.TotalDeliveries++;
                driver.CurrentOrdersCount = Math.Max(0, driver.CurrentOrdersCount - 1);
            }
        }

        await _context.SaveChangesAsync();

        await _hubContext.Clients
            .Group($"order-{orderId}")
            .SendAsync("OrderDelivered", new
            {
                orderId,
                status = RestaurantOrderStatus.Delivered,
                deliveredAt = order.DeliveredAt,
                message = "تم تسليم طلبك"
            });

        return Ok(new
        {
            message = "تم تأكيد التسليم",
            orderId,
            status = RestaurantOrderStatus.Delivered
        });
    }

    /// <summary>
    /// تحديث موقع السائق
    /// </summary>
    [HttpPost("location/{driverId}")]
    public async Task<ActionResult> UpdateLocation(Guid driverId, [FromBody] LocationRequest request)
    {
        var driver = await _context.VendorEmployees
            .FirstOrDefaultAsync(e => e.Id == driverId);

        if (driver == null)
        {
            return NotFound(new { message = "السائق غير موجود" });
        }

        driver.CurrentLatitude = request.Latitude;
        driver.CurrentLongitude = request.Longitude;
        driver.LastLocationUpdate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // إرسال تحديث الموقع لجميع الطلبات النشطة لهذا السائق
        var activeOrders = await _context.RestaurantOrders
            .Where(o => o.AssignedDriverId == driverId &&
                       o.Status == RestaurantOrderStatus.OnTheWay)
            .Select(o => o.Id)
            .ToListAsync();

        foreach (var orderId in activeOrders)
        {
            await _hubContext.Clients
                .Group($"order-{orderId}")
                .SendAsync("DriverLocationUpdated", new
                {
                    latitude = request.Latitude,
                    longitude = request.Longitude
                });
        }

        return Ok(new { message = "تم تحديث الموقع" });
    }

    /// <summary>
    /// تغيير حالة التوفر
    /// </summary>
    [HttpPost("availability/{driverId}")]
    public async Task<ActionResult> ToggleAvailability(Guid driverId)
    {
        var driver = await _context.VendorEmployees
            .FirstOrDefaultAsync(e => e.Id == driverId);

        if (driver == null)
        {
            return NotFound(new { message = "السائق غير موجود" });
        }

        driver.IsAvailable = !driver.IsAvailable;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = driver.IsAvailable ? "أنت الآن متاح للطلبات" : "أنت الآن غير متاح",
            isAvailable = driver.IsAvailable
        });
    }

    /// <summary>
    /// الحصول على إحصائيات السائق
    /// </summary>
    [HttpGet("stats/{driverId}")]
    public async Task<ActionResult> GetStats(Guid driverId)
    {
        var driver = await _context.VendorEmployees
            .FirstOrDefaultAsync(e => e.Id == driverId);

        if (driver == null)
        {
            return NotFound(new { message = "السائق غير موجود" });
        }

        var today = DateTime.UtcNow.Date;
        var todayDeliveries = await _context.OrderDriverAssignments
            .CountAsync(a => a.DriverEmployeeId == driverId &&
                           a.DeliveredAt.HasValue &&
                           a.DeliveredAt.Value.Date == today);

        return Ok(new
        {
            todayDeliveries,
            totalDeliveries = driver.TotalDeliveries,
            averageRating = driver.AverageRating,
            ratingCount = driver.RatingCount,
            isAvailable = driver.IsAvailable,
            currentOrdersCount = driver.CurrentOrdersCount,
            maxConcurrentOrders = driver.MaxConcurrentOrders
        });
    }

    private static string GetStatusDisplay(RestaurantOrderStatus status) => status switch
    {
        RestaurantOrderStatus.AssignedToDriver => "بانتظار الاستلام",
        RestaurantOrderStatus.DriverPickedUp => "تم الاستلام",
        RestaurantOrderStatus.OnTheWay => "في الطريق",
        RestaurantOrderStatus.Delivered => "تم التسليم",
        _ => status.ToString()
    };
}

public class PickupRequest
{
    public string Barcode { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class LocationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class DeliveryConfirmationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? ProofImageUrl { get; set; }
    public string? Notes { get; set; }
}

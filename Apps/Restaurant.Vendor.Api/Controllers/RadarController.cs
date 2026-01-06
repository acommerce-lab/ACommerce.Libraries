using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Restaurant.Core.DTOs;
using Restaurant.Core.Entities;
using Restaurant.Core.Enums;
using Restaurant.Vendor.Api.Hubs;

namespace Restaurant.Vendor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RadarController : ControllerBase
{
    private readonly VendorDbContext _context;
    private readonly IHubContext<VendorOrdersHub> _hubContext;

    public RadarController(VendorDbContext context, IHubContext<VendorOrdersHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    /// <summary>
    /// الحصول على حالة الرادار الحالية
    /// </summary>
    [HttpGet("status/{restaurantId}")]
    public async Task<ActionResult<RadarStatusDto>> GetRadarStatus(Guid restaurantId)
    {
        var restaurant = await _context.RestaurantProfiles
            .FirstOrDefaultAsync(r => r.Id == restaurantId);

        if (restaurant == null)
        {
            return NotFound(new { message = "المطعم غير موجود" });
        }

        // عدد الطلبات حسب الحالة
        var pendingCount = await _context.RestaurantOrders
            .CountAsync(o => o.RestaurantProfileId == restaurantId &&
                           o.Status == RestaurantOrderStatus.WaitingAcceptance);

        var preparingCount = await _context.RestaurantOrders
            .CountAsync(o => o.RestaurantProfileId == restaurantId &&
                           (o.Status == RestaurantOrderStatus.Accepted ||
                            o.Status == RestaurantOrderStatus.Preparing));

        var readyCount = await _context.RestaurantOrders
            .CountAsync(o => o.RestaurantProfileId == restaurantId &&
                           o.Status == RestaurantOrderStatus.Ready);

        return Ok(new RadarStatusDto
        {
            Status = restaurant.CurrentRadarStatus,
            StatusDisplay = GetRadarStatusDisplay(restaurant.CurrentRadarStatus),
            OpenedAt = restaurant.RadarOpenedAt,
            PendingOrdersCount = pendingCount,
            PreparingOrdersCount = preparingCount,
            ReadyOrdersCount = readyCount
        });
    }

    /// <summary>
    /// فتح صفحة الرادار (بدء استلام الطلبات)
    /// </summary>
    [HttpPost("open/{restaurantId}")]
    public async Task<ActionResult> OpenRadar(Guid restaurantId)
    {
        var restaurant = await _context.RestaurantProfiles
            .FirstOrDefaultAsync(r => r.Id == restaurantId);

        if (restaurant == null)
        {
            return NotFound(new { message = "المطعم غير موجود" });
        }

        restaurant.CurrentRadarStatus = RadarStatus.Open;
        restaurant.RadarOpenedAt = DateTime.UtcNow;
        restaurant.RadarLastUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "تم فتح استلام الطلبات",
            status = RadarStatus.Open,
            openedAt = restaurant.RadarOpenedAt
        });
    }

    /// <summary>
    /// تعيين حالة مشغول (لا يستلم طلبات جديدة مؤقتاً)
    /// </summary>
    [HttpPost("busy/{restaurantId}")]
    public async Task<ActionResult> SetBusy(Guid restaurantId)
    {
        var restaurant = await _context.RestaurantProfiles
            .FirstOrDefaultAsync(r => r.Id == restaurantId);

        if (restaurant == null)
        {
            return NotFound(new { message = "المطعم غير موجود" });
        }

        restaurant.CurrentRadarStatus = RadarStatus.Busy;
        restaurant.RadarLastUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "تم تعيين الحالة كمشغول",
            status = RadarStatus.Busy
        });
    }

    /// <summary>
    /// إغلاق صفحة الرادار (إيقاف استلام الطلبات)
    /// </summary>
    [HttpPost("close/{restaurantId}")]
    public async Task<ActionResult> CloseRadar(Guid restaurantId)
    {
        var restaurant = await _context.RestaurantProfiles
            .FirstOrDefaultAsync(r => r.Id == restaurantId);

        if (restaurant == null)
        {
            return NotFound(new { message = "المطعم غير موجود" });
        }

        restaurant.CurrentRadarStatus = RadarStatus.Closed;
        restaurant.RadarOpenedAt = null;
        restaurant.RadarLastUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "تم إغلاق استلام الطلبات",
            status = RadarStatus.Closed
        });
    }

    /// <summary>
    /// الحصول على الطلبات المعلقة (بانتظار القبول)
    /// </summary>
    [HttpGet("pending-orders/{restaurantId}")]
    public async Task<ActionResult<List<RadarOrderDto>>> GetPendingOrders(Guid restaurantId)
    {
        var orders = await _context.RestaurantOrders
            .Include(o => o.Items)
            .Where(o => o.RestaurantProfileId == restaurantId &&
                       o.Status == RestaurantOrderStatus.WaitingAcceptance)
            .OrderBy(o => o.OrderedAt)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var expiryMinutes = 5; // 5 دقائق للقبول

        var result = orders.Select(o => new RadarOrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            OrderedAt = o.OrderedAt,
            SecondsAgo = (int)(now - o.OrderedAt).TotalSeconds,

            CustomerName = o.CustomerName,
            DeliveryAddress = o.DeliveryAddress,
            DistanceKm = o.DistanceKm,

            Total = o.Total,
            ItemsCount = o.Items.Count,

            ItemsSummary = o.Items
                .Select(i => $"{i.Quantity}x {i.ProductName}")
                .Take(5)
                .ToList(),
            CustomerNotes = o.CustomerNotes,

            SecondsUntilExpiry = o.AcceptanceExpiresAt.HasValue
                ? Math.Max(0, (int)(o.AcceptanceExpiresAt.Value - now).TotalSeconds)
                : expiryMinutes * 60 - (int)(now - o.OrderedAt).TotalSeconds
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// قبول طلب
    /// </summary>
    [HttpPost("accept/{orderId}")]
    public async Task<ActionResult> AcceptOrder(Guid orderId)
    {
        var order = await _context.RestaurantOrders
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return NotFound(new { message = "الطلب غير موجود" });
        }

        if (order.Status != RestaurantOrderStatus.WaitingAcceptance)
        {
            return BadRequest(new { message = "لا يمكن قبول هذا الطلب" });
        }

        var previousStatus = order.Status;
        order.Status = RestaurantOrderStatus.Accepted;
        order.AcceptedAt = DateTime.UtcNow;

        // إضافة سجل التغيير
        var history = OrderStatusHistory.Create(
            orderId,
            previousStatus,
            RestaurantOrderStatus.Accepted,
            OrderStatusChangedBy.Vendor,
            notes: "تم قبول الطلب من صفحة الرادار"
        );
        _context.OrderStatusHistories.Add(history);

        await _context.SaveChangesAsync();

        // إرسال إشعار للعميل عبر SignalR
        await _hubContext.Clients
            .Group($"order-{orderId}")
            .SendAsync("OrderAccepted", new
            {
                orderId,
                status = RestaurantOrderStatus.Accepted,
                message = "تم قبول طلبك"
            });

        return Ok(new
        {
            message = "تم قبول الطلب",
            orderId,
            status = RestaurantOrderStatus.Accepted
        });
    }

    /// <summary>
    /// رفض طلب
    /// </summary>
    [HttpPost("reject/{orderId}")]
    public async Task<ActionResult> RejectOrder(Guid orderId, [FromBody] RejectOrderRequest request)
    {
        var order = await _context.RestaurantOrders
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return NotFound(new { message = "الطلب غير موجود" });
        }

        if (order.Status != RestaurantOrderStatus.WaitingAcceptance)
        {
            return BadRequest(new { message = "لا يمكن رفض هذا الطلب" });
        }

        var previousStatus = order.Status;
        order.Status = RestaurantOrderStatus.Rejected;
        order.RejectionReason = request.Reason;

        // إضافة سجل التغيير
        var history = OrderStatusHistory.Create(
            orderId,
            previousStatus,
            RestaurantOrderStatus.Rejected,
            OrderStatusChangedBy.Vendor,
            notes: request.Reason
        );
        _context.OrderStatusHistories.Add(history);

        await _context.SaveChangesAsync();

        // إرسال إشعار للعميل
        await _hubContext.Clients
            .Group($"order-{orderId}")
            .SendAsync("OrderRejected", new
            {
                orderId,
                status = RestaurantOrderStatus.Rejected,
                reason = request.Reason,
                message = "تم رفض طلبك"
            });

        return Ok(new
        {
            message = "تم رفض الطلب",
            orderId,
            status = RestaurantOrderStatus.Rejected
        });
    }

    /// <summary>
    /// بدء تحضير الطلب
    /// </summary>
    [HttpPost("start-preparing/{orderId}")]
    public async Task<ActionResult> StartPreparing(Guid orderId)
    {
        var order = await _context.RestaurantOrders
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return NotFound(new { message = "الطلب غير موجود" });
        }

        if (order.Status != RestaurantOrderStatus.Accepted)
        {
            return BadRequest(new { message = "لا يمكن بدء تحضير هذا الطلب" });
        }

        var previousStatus = order.Status;
        order.Status = RestaurantOrderStatus.Preparing;
        order.PreparationStartedAt = DateTime.UtcNow;

        var history = OrderStatusHistory.Create(
            orderId, previousStatus, RestaurantOrderStatus.Preparing,
            OrderStatusChangedBy.Vendor
        );
        _context.OrderStatusHistories.Add(history);

        await _context.SaveChangesAsync();

        await _hubContext.Clients
            .Group($"order-{orderId}")
            .SendAsync("OrderStatusChanged", new
            {
                orderId,
                status = RestaurantOrderStatus.Preparing,
                message = "طلبك قيد التحضير"
            });

        return Ok(new { message = "تم بدء التحضير", status = RestaurantOrderStatus.Preparing });
    }

    /// <summary>
    /// تعيين الطلب كجاهز
    /// </summary>
    [HttpPost("ready/{orderId}")]
    public async Task<ActionResult> MarkAsReady(Guid orderId)
    {
        var order = await _context.RestaurantOrders
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return NotFound(new { message = "الطلب غير موجود" });
        }

        if (order.Status != RestaurantOrderStatus.Preparing)
        {
            return BadRequest(new { message = "الطلب ليس قيد التحضير" });
        }

        var previousStatus = order.Status;
        order.Status = RestaurantOrderStatus.Ready;
        order.ReadyAt = DateTime.UtcNow;

        var history = OrderStatusHistory.Create(
            orderId, previousStatus, RestaurantOrderStatus.Ready,
            OrderStatusChangedBy.Vendor
        );
        _context.OrderStatusHistories.Add(history);

        await _context.SaveChangesAsync();

        await _hubContext.Clients
            .Group($"order-{orderId}")
            .SendAsync("OrderStatusChanged", new
            {
                orderId,
                status = RestaurantOrderStatus.Ready,
                message = "طلبك جاهز!"
            });

        return Ok(new { message = "الطلب جاهز", status = RestaurantOrderStatus.Ready });
    }

    private static string GetRadarStatusDisplay(RadarStatus status) => status switch
    {
        RadarStatus.Open => "مفتوح",
        RadarStatus.Busy => "مشغول",
        RadarStatus.Closed => "مغلق",
        _ => "غير معروف"
    };
}

public class RejectOrderRequest
{
    public string? Reason { get; set; }
}

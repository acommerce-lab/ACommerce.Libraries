using Microsoft.AspNetCore.SignalR;

namespace Restaurant.Customer.Api.Hubs;

/// <summary>
/// Hub لتتبع الطلبات في الوقت الفعلي
/// </summary>
public class OrderTrackingHub : Hub
{
    /// <summary>
    /// الانضمام لمجموعة تتبع طلب معين
    /// </summary>
    public async Task JoinOrderTracking(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }

    /// <summary>
    /// مغادرة مجموعة تتبع طلب
    /// </summary>
    public async Task LeaveOrderTracking(string orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }

    /// <summary>
    /// الانضمام لمجموعة تتبع جميع طلبات عميل
    /// </summary>
    public async Task JoinCustomerOrders(string customerId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"customer-{customerId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}

/// <summary>
/// خدمة إرسال تحديثات الطلبات
/// </summary>
public class OrderTrackingNotifier
{
    private readonly IHubContext<OrderTrackingHub> _hubContext;

    public OrderTrackingNotifier(IHubContext<OrderTrackingHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// إرسال تحديث حالة الطلب
    /// </summary>
    public async Task NotifyOrderStatusChanged(Guid orderId, object statusUpdate)
    {
        await _hubContext.Clients
            .Group($"order-{orderId}")
            .SendAsync("OrderStatusChanged", statusUpdate);
    }

    /// <summary>
    /// إرسال تحديث موقع السائق
    /// </summary>
    public async Task NotifyDriverLocationUpdated(Guid orderId, double latitude, double longitude)
    {
        await _hubContext.Clients
            .Group($"order-{orderId}")
            .SendAsync("DriverLocationUpdated", new { latitude, longitude });
    }

    /// <summary>
    /// إرسال إشعار للعميل
    /// </summary>
    public async Task NotifyCustomer(string customerId, string type, object data)
    {
        await _hubContext.Clients
            .Group($"customer-{customerId}")
            .SendAsync("CustomerNotification", new { type, data });
    }
}

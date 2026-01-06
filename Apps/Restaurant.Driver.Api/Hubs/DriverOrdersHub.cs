using Microsoft.AspNetCore.SignalR;

namespace Restaurant.Driver.Api.Hubs;

/// <summary>
/// Hub لإشعارات السائق في الوقت الفعلي
/// </summary>
public class DriverOrdersHub : Hub
{
    /// <summary>
    /// انضمام السائق لمجموعته
    /// </summary>
    public async Task JoinDriver(string driverId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"driver-{driverId}");
    }

    /// <summary>
    /// الانضمام لتتبع طلب
    /// </summary>
    public async Task JoinOrder(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}

/// <summary>
/// خدمة إرسال إشعارات للسائق
/// </summary>
public class DriverNotifier
{
    private readonly IHubContext<DriverOrdersHub> _hubContext;

    public DriverNotifier(IHubContext<DriverOrdersHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// إشعار السائق بطلب جديد
    /// </summary>
    public async Task NotifyNewAssignment(Guid driverId, object orderData)
    {
        await _hubContext.Clients
            .Group($"driver-{driverId}")
            .SendAsync("NewAssignment", orderData);
    }

    /// <summary>
    /// إشعار بإلغاء طلب
    /// </summary>
    public async Task NotifyOrderCancelled(Guid driverId, Guid orderId, string reason)
    {
        await _hubContext.Clients
            .Group($"driver-{driverId}")
            .SendAsync("OrderCancelled", new { orderId, reason });
    }
}

using Microsoft.AspNetCore.SignalR;

namespace Restaurant.Vendor.Api.Hubs;

/// <summary>
/// Hub لإشعارات المطعم في الوقت الفعلي
/// </summary>
public class VendorOrdersHub : Hub
{
    /// <summary>
    /// انضمام المطعم لمجموعته
    /// </summary>
    public async Task JoinRestaurant(string restaurantId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"restaurant-{restaurantId}");
    }

    /// <summary>
    /// مغادرة مجموعة المطعم
    /// </summary>
    public async Task LeaveRestaurant(string restaurantId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"restaurant-{restaurantId}");
    }

    /// <summary>
    /// الانضمام لتتبع طلب محدد
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
/// خدمة إرسال إشعارات للمطعم
/// </summary>
public class VendorNotifier
{
    private readonly IHubContext<VendorOrdersHub> _hubContext;

    public VendorNotifier(IHubContext<VendorOrdersHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// إرسال إشعار طلب جديد
    /// </summary>
    public async Task NotifyNewOrder(Guid restaurantId, object orderData)
    {
        await _hubContext.Clients
            .Group($"restaurant-{restaurantId}")
            .SendAsync("NewOrder", orderData);
    }

    /// <summary>
    /// إرسال تحديث حالة طلب
    /// </summary>
    public async Task NotifyOrderUpdate(Guid restaurantId, object orderUpdate)
    {
        await _hubContext.Clients
            .Group($"restaurant-{restaurantId}")
            .SendAsync("OrderUpdated", orderUpdate);
    }
}

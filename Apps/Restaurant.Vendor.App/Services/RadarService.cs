using Microsoft.AspNetCore.SignalR.Client;

namespace Restaurant.Vendor.App.Services;

/// <summary>
/// خدمة الرادار - استقبال الطلبات في الوقت الفعلي
/// </summary>
public class RadarService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly string _hubUrl;
    private Guid? _currentRestaurantId;

    public event Action<NewOrderNotification>? OnNewOrder;
    public event Action<OrderUpdateNotification>? OnOrderUpdated;
    public event Action<bool>? OnConnectionChanged;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public RadarService()
    {
        _hubUrl = $"{ApiSettings.BaseUrl}/hubs/vendor-orders";
    }

    public async Task ConnectAsync(Guid restaurantId)
    {
        _currentRestaurantId = restaurantId;

        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl, options =>
            {
#if DEBUG
                options.HttpMessageHandlerFactory = _ => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                };
#endif
            })
            .WithAutomaticReconnect()
            .Build();

        // الاستماع لطلب جديد
        _hubConnection.On<NewOrderNotification>("NewOrder", order =>
        {
            Console.WriteLine($"[Radar] New order received: {order.OrderNumber}");
            OnNewOrder?.Invoke(order);
        });

        // الاستماع لتحديث طلب
        _hubConnection.On<OrderUpdateNotification>("OrderUpdated", update =>
        {
            Console.WriteLine($"[Radar] Order updated: {update.OrderId}");
            OnOrderUpdated?.Invoke(update);
        });

        _hubConnection.Reconnecting += _ =>
        {
            Console.WriteLine("[Radar] Reconnecting...");
            OnConnectionChanged?.Invoke(false);
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += _ =>
        {
            Console.WriteLine("[Radar] Reconnected");
            OnConnectionChanged?.Invoke(true);
            // إعادة الانضمام لمجموعة المطعم
            if (_currentRestaurantId.HasValue)
            {
                _ = JoinRestaurantGroupAsync(_currentRestaurantId.Value);
            }
            return Task.CompletedTask;
        };

        await _hubConnection.StartAsync();
        await JoinRestaurantGroupAsync(restaurantId);

        OnConnectionChanged?.Invoke(true);
        Console.WriteLine($"[Radar] Connected for restaurant: {restaurantId}");
    }

    private async Task JoinRestaurantGroupAsync(Guid restaurantId)
    {
        if (_hubConnection == null) return;
        await _hubConnection.InvokeAsync("JoinRestaurant", restaurantId.ToString());
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection == null) return;

        if (_currentRestaurantId.HasValue)
        {
            await _hubConnection.InvokeAsync("LeaveRestaurant", _currentRestaurantId.Value.ToString());
        }

        await _hubConnection.StopAsync();
        OnConnectionChanged?.Invoke(false);
        Console.WriteLine("[Radar] Disconnected");
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}

public class NewOrderNotification
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int ItemsCount { get; set; }
    public DateTime OrderedAt { get; set; }
}

public class OrderUpdateNotification
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

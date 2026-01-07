using Microsoft.AspNetCore.SignalR.Client;

namespace Restaurant.Driver.App.Services;

/// <summary>
/// خدمة إدارة طلبات السائق في الوقت الفعلي
/// </summary>
public class DriverOrderService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly string _hubUrl;
    private Guid? _currentDriverId;

    public event Action<AssignedOrderNotification>? OnOrderAssigned;
    public event Action<OrderCancelledNotification>? OnOrderCancelled;
    public event Action<bool>? OnConnectionChanged;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public DriverOrderService()
    {
        _hubUrl = $"{ApiSettings.BaseUrl}/hubs/driver-orders";
    }

    public async Task ConnectAsync(Guid driverId)
    {
        _currentDriverId = driverId;

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

        // الاستماع لطلب جديد معين للسائق
        _hubConnection.On<AssignedOrderNotification>("OrderAssigned", order =>
        {
            Console.WriteLine($"[DriverOrder] Order assigned: {order.OrderNumber}");
            OnOrderAssigned?.Invoke(order);
        });

        // الاستماع لإلغاء طلب
        _hubConnection.On<OrderCancelledNotification>("OrderCancelled", notification =>
        {
            Console.WriteLine($"[DriverOrder] Order cancelled: {notification.OrderId}");
            OnOrderCancelled?.Invoke(notification);
        });

        _hubConnection.Reconnecting += _ =>
        {
            Console.WriteLine("[DriverOrder] Reconnecting...");
            OnConnectionChanged?.Invoke(false);
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += _ =>
        {
            Console.WriteLine("[DriverOrder] Reconnected");
            OnConnectionChanged?.Invoke(true);
            if (_currentDriverId.HasValue)
            {
                _ = JoinDriverGroupAsync(_currentDriverId.Value);
            }
            return Task.CompletedTask;
        };

        await _hubConnection.StartAsync();
        await JoinDriverGroupAsync(driverId);

        OnConnectionChanged?.Invoke(true);
        Console.WriteLine($"[DriverOrder] Connected for driver: {driverId}");
    }

    private async Task JoinDriverGroupAsync(Guid driverId)
    {
        if (_hubConnection == null) return;
        await _hubConnection.InvokeAsync("JoinDriver", driverId.ToString());
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection == null) return;

        if (_currentDriverId.HasValue)
        {
            await _hubConnection.InvokeAsync("LeaveDriver", _currentDriverId.Value.ToString());
        }

        await _hubConnection.StopAsync();
        OnConnectionChanged?.Invoke(false);
        Console.WriteLine("[DriverOrder] Disconnected");
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}

public class AssignedOrderNotification
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string RestaurantName { get; set; } = string.Empty;
    public string RestaurantAddress { get; set; } = string.Empty;
    public double RestaurantLatitude { get; set; }
    public double RestaurantLongitude { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public double CustomerLatitude { get; set; }
    public double CustomerLongitude { get; set; }
    public int ItemsCount { get; set; }
    public decimal Total { get; set; }
}

public class OrderCancelledNotification
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

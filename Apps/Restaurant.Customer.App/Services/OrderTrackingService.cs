using Microsoft.AspNetCore.SignalR.Client;

namespace Restaurant.Customer.App.Services;

/// <summary>
/// خدمة تتبع الطلب في الوقت الفعلي
/// </summary>
public class OrderTrackingService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly string _hubUrl;

    public event Action<OrderStatusUpdate>? OnStatusChanged;
    public event Action<DriverLocationUpdate>? OnDriverLocationChanged;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public OrderTrackingService()
    {
        _hubUrl = $"{ApiSettings.BaseUrl}/hubs/order-tracking";
    }

    public async Task ConnectAsync()
    {
        if (_hubConnection != null) return;

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

        // الاستماع لتحديثات حالة الطلب
        _hubConnection.On<OrderStatusUpdate>("OrderStatusChanged", update =>
        {
            Console.WriteLine($"[OrderTracking] Status changed: {update.Status}");
            OnStatusChanged?.Invoke(update);
        });

        // الاستماع لتحديثات موقع السائق
        _hubConnection.On<DriverLocationUpdate>("DriverLocationUpdated", update =>
        {
            Console.WriteLine($"[OrderTracking] Driver at: {update.Latitude}, {update.Longitude}");
            OnDriverLocationChanged?.Invoke(update);
        });

        await _hubConnection.StartAsync();
        Console.WriteLine("[OrderTracking] Connected to hub");
    }

    public async Task TrackOrderAsync(Guid orderId)
    {
        if (_hubConnection == null) await ConnectAsync();

        await _hubConnection!.InvokeAsync("JoinOrderGroup", orderId.ToString());
        Console.WriteLine($"[OrderTracking] Tracking order: {orderId}");
    }

    public async Task StopTrackingAsync(Guid orderId)
    {
        if (_hubConnection == null) return;

        await _hubConnection.InvokeAsync("LeaveOrderGroup", orderId.ToString());
        Console.WriteLine($"[OrderTracking] Stopped tracking: {orderId}");
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}

public class OrderStatusUpdate
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public class DriverLocationUpdate
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int? EstimatedMinutes { get; set; }
}

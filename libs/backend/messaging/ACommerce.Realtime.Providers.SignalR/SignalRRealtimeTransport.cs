using ACommerce.Realtime.Abstractions.Contracts;
using ACommerce.Realtime.Operations.Abstractions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ACommerce.Realtime.Providers.SignalR;

/// <summary>
/// مزود SignalR لـ IRealtimeTransport.
/// يجسر بين تجريدات OperationEngine وـ ASP.NET Core SignalR.
///
/// الاستخدام:
///   services.AddSignalRRealtimeTransport&lt;MyHub, IMyClient&gt;()
/// </summary>
public class SignalRRealtimeTransport<THub, TClient> : IRealtimeTransport
    where THub : Hub<TClient>
    where TClient : class, IRealtimeClient
{
    private readonly IHubContext<THub, TClient> _hubContext;
    private readonly ILogger<SignalRRealtimeTransport<THub, TClient>> _logger;

    public SignalRRealtimeTransport(
        IHubContext<THub, TClient> hubContext,
        ILogger<SignalRRealtimeTransport<THub, TClient>> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task SendToUserAsync(string userId, string method, object data, CancellationToken ct = default)
    {
        try
        {
            // المستخدم مضاف لمجموعة باسمه عند الاتصال
            await _hubContext.Clients.Group(userId).ReceiveMessage(method, data);
            _logger.LogDebug("[SignalR] Sent {Method} to user {UserId}", method, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SignalR] Failed to send {Method} to user {UserId}", method, userId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SendToGroupAsync(string groupName, string method, object data, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Clients.Group(groupName).ReceiveMessage(method, data);
            _logger.LogDebug("[SignalR] Sent {Method} to group {Group}", method, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SignalR] Failed to send {Method} to group {Group}", method, groupName);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task BroadcastAsync(string method, object data, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Clients.All.ReceiveMessage(method, data);
            _logger.LogDebug("[SignalR] Broadcast {Method} to all clients", method);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SignalR] Failed to broadcast {Method}", method);
            throw;
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// AddToGroupAsync يتطلب connectionId - يجب استدعاؤه من داخل Hub عادةً.
    /// من خارج Hub، يمكن استخدامه إذا كان connectionId معروفاً.
    /// </remarks>
    public async Task AddToGroupAsync(string connectionId, string groupName, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Groups.AddToGroupAsync(connectionId, groupName, ct);
            _logger.LogDebug("[SignalR] Added connection {ConnectionId} to group {Group}", connectionId, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SignalR] Failed to add {ConnectionId} to group {Group}", connectionId, groupName);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName, ct);
            _logger.LogDebug("[SignalR] Removed connection {ConnectionId} from group {Group}", connectionId, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SignalR] Failed to remove {ConnectionId} from group {Group}", connectionId, groupName);
            throw;
        }
    }
}

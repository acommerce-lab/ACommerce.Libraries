using ACommerce.OperationEngine.Core;
using ACommerce.Realtime.Operations.Abstractions;
using ACommerce.Realtime.Operations.Operations;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Realtime.Operations;

/// <summary>
/// واجهة الزمن الحقيقي البسيطة.
///
/// services.AddRealtime(transport);  // مرة واحدة
///
/// ثم:
///   await realtime.SendToUserAsync("user:123", "NewOrder", orderData);
///   await realtime.BroadcastAsync("SystemAlert", alertData);
/// </summary>
public class RealtimeService
{
    private readonly IRealtimeTransport _transport;
    private readonly IConnectionTracker? _tracker;
    private readonly OpEngine _engine;

    public RealtimeService(IRealtimeTransport transport, OpEngine engine, IConnectionTracker? tracker = null)
    {
        _transport = transport;
        _engine = engine;
        _tracker = tracker;
    }

    public async Task<OperationResult> SendToUserAsync(string userId, string method, object data,
        string? senderId = null, CancellationToken ct = default)
    {
        var op = ConnectionOps.SendToUser(userId, method, data, _transport, senderId);
        return await _engine.ExecuteAsync(op, ct);
    }

    public async Task<OperationResult> BroadcastAsync(string method, object data, CancellationToken ct = default)
    {
        var op = ConnectionOps.Broadcast(method, data, _transport);
        return await _engine.ExecuteAsync(op, ct);
    }

    public async Task<OperationResult> OnConnectedAsync(string userId, string connectionId, CancellationToken ct = default)
    {
        var op = ConnectionOps.Connect(userId, connectionId, _tracker);
        return await _engine.ExecuteAsync(op, ct);
    }

    public async Task<OperationResult> OnDisconnectedAsync(string userId, CancellationToken ct = default)
    {
        var op = ConnectionOps.Disconnect(userId, _tracker);
        return await _engine.ExecuteAsync(op, ct);
    }

    public async Task<OperationResult> JoinGroupAsync(string userId, string connectionId, string groupName, CancellationToken ct = default)
    {
        var op = ConnectionOps.JoinGroup(userId, connectionId, groupName, _transport);
        return await _engine.ExecuteAsync(op, ct);
    }

    public async Task<OperationResult> LeaveGroupAsync(string userId, string connectionId, string groupName, CancellationToken ct = default)
    {
        var op = ConnectionOps.LeaveGroup(userId, connectionId, groupName, _transport);
        return await _engine.ExecuteAsync(op, ct);
    }

    /// <summary>
    /// هل المستخدم متصل؟
    /// </summary>
    public Task<bool> IsOnlineAsync(string userId, CancellationToken ct = default)
        => _tracker?.IsOnlineAsync(userId, ct) ?? Task.FromResult(false);
}

public static class RealtimeExtensions
{
    public static IServiceCollection AddRealtime<TTransport>(this IServiceCollection services)
        where TTransport : class, IRealtimeTransport
    {
        services.AddScoped<IRealtimeTransport, TTransport>();
        services.AddScoped<RealtimeService>();
        return services;
    }

    public static IServiceCollection AddRealtime(this IServiceCollection services, IRealtimeTransport transport)
    {
        services.AddSingleton(transport);
        services.AddScoped<RealtimeService>();
        return services;
    }

    public static IServiceCollection AddConnectionTracker<TTracker>(this IServiceCollection services)
        where TTracker : class, IConnectionTracker
    {
        services.AddScoped<IConnectionTracker, TTracker>();
        return services;
    }
}

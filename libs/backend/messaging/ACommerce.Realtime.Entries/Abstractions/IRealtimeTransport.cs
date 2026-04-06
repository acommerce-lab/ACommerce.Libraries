namespace ACommerce.Realtime.Entries.Abstractions;

/// <summary>
/// تجريد النقل في الزمن الحقيقي.
/// لا نعتمد على SignalR أو أي مزود - هذا عقد مجرد.
/// التطبيق يوفره الطبقة العليا (SignalR, WebSocket, gRPC, etc.)
/// </summary>
public interface IRealtimeTransport
{
    /// <summary>
    /// إرسال رسالة لمستخدم محدد
    /// </summary>
    Task SendToUserAsync(string userId, string method, object data, CancellationToken ct = default);

    /// <summary>
    /// إرسال رسالة لمجموعة
    /// </summary>
    Task SendToGroupAsync(string groupName, string method, object data, CancellationToken ct = default);

    /// <summary>
    /// إرسال لجميع المتصلين
    /// </summary>
    Task BroadcastAsync(string method, object data, CancellationToken ct = default);

    /// <summary>
    /// إضافة مستخدم لمجموعة
    /// </summary>
    Task AddToGroupAsync(string connectionId, string groupName, CancellationToken ct = default);

    /// <summary>
    /// إزالة مستخدم من مجموعة
    /// </summary>
    Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken ct = default);
}

/// <summary>
/// تجريد تخزين الاتصالات (أي مستخدم متصل على أي اتصال)
/// </summary>
public interface IConnectionStore
{
    Task<string?> GetConnectionIdAsync(string userId, CancellationToken ct = default);
    Task SetConnectionAsync(string userId, string connectionId, CancellationToken ct = default);
    Task RemoveConnectionAsync(string userId, CancellationToken ct = default);
    Task<bool> IsConnectedAsync(string userId, CancellationToken ct = default);
}

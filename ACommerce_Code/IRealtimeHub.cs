namespace ACommerce.Realtime.Abstractions.Contracts;

/// <summary>
/// Interface for real-time hub service
/// </summary>
public interface IRealtimeHub
{
	Task SendToUserAsync(string userId, string method, object data, CancellationToken cancellationToken = default);
	Task SendToGroupAsync(string groupName, string method, object data, CancellationToken cancellationToken = default);
	Task SendToAllAsync(string method, object data, CancellationToken cancellationToken = default);
	Task AddToGroupAsync(string userId, string groupName, CancellationToken cancellationToken = default);
	Task RemoveFromGroupAsync(string userId, string groupName, CancellationToken cancellationToken = default);
}


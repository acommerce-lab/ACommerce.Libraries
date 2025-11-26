using ACommerce.Realtime.Abstractions.Contracts;
using Microsoft.Extensions.Logging;

namespace ACommerce.Realtime.SignalR.Hubs;

/// <summary>
/// SignalR Hub for real-time notifications
/// </summary>
public class NotificationHub : BaseRealtimeHub<INotificationClient>
{
	public NotificationHub(ILogger<NotificationHub> logger) : base(logger)
	{
	}

	/// <summary>
	/// Subscribe to a specific notification topic
	/// </summary>
	public async Task SubscribeToTopic(string topic)
	{
		var userId = GetUserId();
		if (string.IsNullOrEmpty(userId))
		{
			Logger.LogWarning("Anonymous user tried to subscribe to topic {Topic}", topic);
			return;
		}

		await Groups.AddToGroupAsync(Context.ConnectionId, $"topic_{topic}");
		Logger.LogInformation("User {UserId} subscribed to topic {Topic}", userId, topic);
	}

	/// <summary>
	/// Unsubscribe from a specific notification topic
	/// </summary>
	public async Task UnsubscribeFromTopic(string topic)
	{
		var userId = GetUserId();
		if (string.IsNullOrEmpty(userId))
		{
			return;
		}

		await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"topic_{topic}");
		Logger.LogInformation("User {UserId} unsubscribed from topic {Topic}", userId, topic);
	}

	/// <summary>
	/// Mark notification as read
	/// </summary>
	public async Task MarkAsRead(string notificationId)
	{
		var userId = GetUserId();
		if (string.IsNullOrEmpty(userId))
		{
			return;
		}

		Logger.LogInformation("User {UserId} marked notification {NotificationId} as read", userId, notificationId);

		// Notify other connections of the same user
		await Clients.OthersInGroup(userId).ReceiveMessage("NotificationRead", new
		{
			NotificationId = notificationId
		});
	}

	protected override async Task OnUserConnectedAsync(string userId)
	{
		Logger.LogInformation("User {UserId} connected to notification system", userId);

		// Notify clients about connection
		await Clients.Caller.ReceiveMessage("Connected", new
		{
			UserId = userId,
			ConnectionId = Context.ConnectionId
		});
	}

	protected override async Task OnUserDisconnectedAsync(string userId, Exception? exception)
	{
		Logger.LogInformation("User {UserId} disconnected from notification system", userId);
	}
}

/// <summary>
/// Client interface for notification hub
/// </summary>
public interface INotificationClient : IRealtimeClient
{
	// Inherits ReceiveMessage from IRealtimeClient
}

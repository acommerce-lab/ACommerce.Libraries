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
	/// Subscribe to authentication notifications for a specific identifier (NationalId)
	/// This allows anonymous clients to receive auth success/failure notifications
	/// </summary>
	public async Task SubscribeToAuth(string identifier)
	{
		if (string.IsNullOrEmpty(identifier))
		{
			Logger.LogWarning("SubscribeToAuth called with empty identifier");
			return;
		}

		// Add connection to a group based on the identifier
		// This allows the client to receive notifications when auth succeeds/fails
		var groupName = $"auth_{identifier}";
		await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

		Logger.LogInformation(
			"Connection {ConnectionId} subscribed to auth notifications for {Identifier}",
			Context.ConnectionId,
			identifier);

		// Confirm subscription to client
		await Clients.Caller.ReceiveMessage("AuthSubscribed", new
		{
			Identifier = identifier,
			GroupName = groupName
		});
	}

	/// <summary>
	/// Unsubscribe from authentication notifications
	/// </summary>
	public async Task UnsubscribeFromAuth(string identifier)
	{
		if (string.IsNullOrEmpty(identifier))
		{
			return;
		}

		var groupName = $"auth_{identifier}";
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

		Logger.LogInformation(
			"Connection {ConnectionId} unsubscribed from auth notifications for {Identifier}",
			Context.ConnectionId,
			identifier);
	}

	/// <summary>
	/// Subscribe to payment notifications for a specific payment ID
	/// This allows clients to receive payment success/failure notifications
	/// </summary>
	public async Task SubscribeToPayment(string paymentId)
	{
		if (string.IsNullOrEmpty(paymentId))
		{
			Logger.LogWarning("SubscribeToPayment called with empty paymentId");
			return;
		}

		var groupName = $"payment_{paymentId}";
		await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

		Logger.LogInformation(
			"Connection {ConnectionId} subscribed to payment notifications for {PaymentId}",
			Context.ConnectionId,
			paymentId);

		// Confirm subscription to client
		await Clients.Caller.ReceiveMessage("PaymentSubscribed", new
		{
			PaymentId = paymentId,
			GroupName = groupName
		});
	}

	/// <summary>
	/// Unsubscribe from payment notifications
	/// </summary>
	public async Task UnsubscribeFromPayment(string paymentId)
	{
		if (string.IsNullOrEmpty(paymentId))
		{
			return;
		}

		var groupName = $"payment_{paymentId}";
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

		Logger.LogInformation(
			"Connection {ConnectionId} unsubscribed from payment notifications for {PaymentId}",
			Context.ConnectionId,
			paymentId);
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

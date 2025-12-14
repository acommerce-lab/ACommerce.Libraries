using ACommerce.Realtime.Abstractions.Contracts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ACommerce.Realtime.SignalR.Hubs;

/// <summary>
/// Base SignalR Hub for real-time communication
/// Can be inherited for specific use cases (Nafath, Notifications, Chat, etc.)
/// </summary>
public abstract class BaseRealtimeHub<TClient> : Hub<TClient>
	where TClient : class, IRealtimeClient
{
	protected readonly ILogger Logger;

	protected BaseRealtimeHub(ILogger logger)
	{
		Logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public override async Task OnConnectedAsync()
	{
		try
		{
			var userId = GetUserId();

			if (!string.IsNullOrEmpty(userId))
			{
				// Add user to their personal group
				await Groups.AddToGroupAsync(Context.ConnectionId, userId);

				Logger.LogInformation(
					"User {UserId} connected to hub with connection {ConnectionId}",
					userId,
					Context.ConnectionId);

				await OnUserConnectedAsync(userId);
			}
			else
			{
				Logger.LogDebug("Anonymous connection - allowed for auth subscriptions");
			}

			await base.OnConnectedAsync();
		}
		catch (Exception ex)
		{
			// ✅ لا نرمي الخطأ - نسجله فقط ونستمر
			Logger.LogError(ex, "Error during connection for {ConnectionId}", Context.ConnectionId);
			// لا throw - السماح للاتصال بالاستمرار
		}
	}

	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		try
		{
			var userId = GetUserId();

			if (!string.IsNullOrEmpty(userId))
			{
				await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);

				Logger.LogInformation(
					"User {UserId} disconnected from hub",
					userId);

				await OnUserDisconnectedAsync(userId, exception);
			}

			await base.OnDisconnectedAsync(exception);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Error during disconnection");
		}
	}

	/// <summary>
	/// Override to implement custom logic when user connects
	/// </summary>
	protected virtual Task OnUserConnectedAsync(string userId) => Task.CompletedTask;

	/// <summary>
	/// Override to implement custom logic when user disconnects
	/// </summary>
	protected virtual Task OnUserDisconnectedAsync(string userId, Exception? exception) => Task.CompletedTask;

	/// <summary>
	/// Override to provide custom user ID extraction
	/// </summary>
	protected virtual string? GetUserId()
	{
		return Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
	}
}


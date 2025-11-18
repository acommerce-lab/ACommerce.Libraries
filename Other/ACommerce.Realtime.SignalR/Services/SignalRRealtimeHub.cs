using ACommerce.Realtime.Abstractions.Contracts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ACommerce.Realtime.SignalR.Services;

/// <summary>
/// Generic SignalR hub service implementation
/// </summary>
public class SignalRRealtimeHub<THub, TClient> : IRealtimeHub
	where THub : Hub<TClient>
	where TClient : class, IRealtimeClient
{
	private readonly IHubContext<THub, TClient> _hubContext;
	private readonly ILogger<SignalRRealtimeHub<THub, TClient>> _logger;

	public SignalRRealtimeHub(
		IHubContext<THub, TClient> hubContext,
		ILogger<SignalRRealtimeHub<THub, TClient>> logger)
	{
		_hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task SendToUserAsync(
		string userId,
		string method,
		object data,
		CancellationToken cancellationToken = default)
	{
		try
		{
			await _hubContext.Clients
				.Group(userId)
				.ReceiveMessage(method, data);

			_logger.LogDebug(
				"Sent message to user {UserId} via method {Method}",
				userId,
				method);
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Failed to send message to user {UserId}",
				userId);
			throw;
		}
	}

	public async Task SendToGroupAsync(
		string groupName,
		string method,
		object data,
		CancellationToken cancellationToken = default)
	{
		try
		{
			await _hubContext.Clients
				.Group(groupName)
				.ReceiveMessage(method, data);

			_logger.LogDebug(
				"Sent message to group {GroupName} via method {Method}",
				groupName,
				method);
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Failed to send message to group {GroupName}",
				groupName);
			throw;
		}
	}

	public async Task SendToAllAsync(
		string method,
		object data,
		CancellationToken cancellationToken = default)
	{
		try
		{
			await _hubContext.Clients.All
				.ReceiveMessage(method, data);

			_logger.LogDebug("Sent message to all clients via method {Method}", method);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send message to all clients");
			throw;
		}
	}

	public async Task AddToGroupAsync(
		string userId,
		string groupName,
		CancellationToken cancellationToken = default)
	{
		try
		{
			// Note: This requires knowing the connection ID
			// In practice, this should be called from within the Hub
			_logger.LogWarning(
				"AddToGroupAsync called from service - should be called from Hub context");

			await Task.CompletedTask;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to add user to group");
			throw;
		}
	}

	public async Task RemoveFromGroupAsync(
		string userId,
		string groupName,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogWarning(
				"RemoveFromGroupAsync called from service - should be called from Hub context");

			await Task.CompletedTask;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to remove user from group");
			throw;
		}
	}
}


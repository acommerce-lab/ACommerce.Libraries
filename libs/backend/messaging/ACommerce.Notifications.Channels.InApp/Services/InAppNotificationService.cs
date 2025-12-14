using ACommerce.Notifications.Abstractions.Enums;
using ACommerce.Notifications.Channels.InApp.Options;
using ACommerce.Realtime.Abstractions.Contracts;
using Microsoft.Extensions.Logging;

namespace ACommerce.Notifications.Channels.InApp.Services;

/// <summary>
/// ???? ?????? ?????? ????????? ???? ???????
/// ???? ????? ??? ????? ??? ????????? ??? ????????
/// </summary>
public class InAppNotificationService
{
	private readonly IRealtimeHub _realtimeHub;
	private readonly ILogger<InAppNotificationService> _logger;
	private readonly InAppNotificationOptions _options;

	public InAppNotificationService(
		IRealtimeHub realtimeHub,
		InAppNotificationOptions options,
		ILogger<InAppNotificationService> logger)
	{
		_realtimeHub = realtimeHub ?? throw new ArgumentNullException(nameof(realtimeHub));
		_options = options ?? throw new ArgumentNullException(nameof(options));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// ????? ??? ????????? ??? ???????? ????????
	/// </summary>
	public async Task SendBadgeCountAsync(
		string userId,
		int count,
		CancellationToken cancellationToken = default)
	{
		if (!_options.SendBadgeCount)
		{
			_logger.LogDebug("Badge count sending is disabled");
			return;
		}

		try
		{
			_logger.LogDebug(
				"Sending badge count {Count} to user {UserId}",
				count,
				userId);

			await _realtimeHub.SendToUserAsync(
				userId,
				_options.BadgeCountMethodName,
				new { count },
				cancellationToken);

			_logger.LogInformation(
				"Badge count {Count} sent successfully to user {UserId}",
				count,
				userId);
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Failed to send badge count to user {UserId}",
				userId);
		}
	}

	/// <summary>
	/// ????? ????? ????? (???? ?????? ?? NotificationService)
	/// ???? ????????? ??????? ????
	/// </summary>
	public async Task SendDirectNotificationAsync(
		string userId,
		string title,
		string message,
		NotificationType type = NotificationType.Info,
		Dictionary<string, string>? data = null,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogDebug(
				"Sending direct notification to user {UserId}",
				userId);

			var payload = new
			{
				id = Guid.NewGuid(),
				userId,
				title,
				message,
				type = type.ToString(),
				createdAt = DateTimeOffset.UtcNow,
				data
			};

			await _realtimeHub.SendToUserAsync(
				userId,
				_options.MethodName,
				payload,
				cancellationToken);

			_logger.LogInformation(
				"Direct notification sent successfully to user {UserId}",
				userId);
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Failed to send direct notification to user {UserId}",
				userId);
		}
	}

	/// <summary>
	/// ????? ????? ??????? ?? ??????????
	/// </summary>
	public async Task SendToGroupAsync(
		string groupName,
		string title,
		string message,
		NotificationType type = NotificationType.Info,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogDebug(
				"Sending notification to group {GroupName}",
				groupName);

			var payload = new
			{
				id = Guid.NewGuid(),
				title,
				message,
				type = type.ToString(),
				createdAt = DateTimeOffset.UtcNow
			};

			await _realtimeHub.SendToGroupAsync(
				groupName,
				_options.MethodName,
				payload,
				cancellationToken);

			_logger.LogInformation(
				"Notification sent successfully to group {GroupName}",
				groupName);
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Failed to send notification to group {GroupName}",
				groupName);
		}
	}

	/// <summary>
	/// ????? broadcast ????? ?????????? ????????
	/// </summary>
	public async Task BroadcastNotificationAsync(
		string title,
		string message,
		NotificationType type = NotificationType.SystemAlert,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Broadcasting notification to all users");

			var payload = new
			{
				id = Guid.NewGuid(),
				title,
				message,
				type = type.ToString(),
				createdAt = DateTimeOffset.UtcNow
			};

			await _realtimeHub.SendToAllAsync(
				_options.MethodName,
				payload,
				cancellationToken);

			_logger.LogInformation("Broadcast notification sent successfully");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to broadcast notification");
		}
	}
}


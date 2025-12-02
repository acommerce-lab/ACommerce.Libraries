using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Abstractions.Enums;
using ACommerce.Notifications.Abstractions.Models;
using ACommerce.Notifications.Channels.InApp.Options;
using ACommerce.Realtime.Abstractions.Contracts;
using Microsoft.Extensions.Logging;

namespace ACommerce.Notifications.Channels.InApp;

/// <summary>
/// ???? ????? ????????? ???? ??????? ??? SignalR
/// </summary>
public class InAppNotificationChannel : INotificationChannel
{
	private readonly IRealtimeHub _realtimeHub;
	private readonly ILogger<InAppNotificationChannel> _logger;
	private readonly InAppNotificationOptions _options;

	public NotificationChannel Channel => NotificationChannel.InApp;

	public InAppNotificationChannel(
		IRealtimeHub realtimeHub,
		InAppNotificationOptions options,
		ILogger<InAppNotificationChannel> logger)
	{
		_realtimeHub = realtimeHub ?? throw new ArgumentNullException(nameof(realtimeHub));
		_options = options ?? throw new ArgumentNullException(nameof(options));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<NotificationResult> SendAsync(
		Notification notification,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogDebug(
				"Sending InApp notification {NotificationId} to user {UserId}",
				notification.Id,
				notification.UserId);

			var payload = BuildNotificationPayload(notification);

			// Send to authenticated user (for logged-in users)
			await _realtimeHub.SendToUserAsync(
				notification.UserId,
				_options.MethodName,
				payload,
				cancellationToken);

			// Also send to auth group (for anonymous clients waiting for authentication)
			// This allows clients who called SubscribeToAuth(identifier) to receive notifications
			var authGroupName = $"auth_{notification.UserId}";
			await _realtimeHub.SendToGroupAsync(
				authGroupName,
				_options.MethodName,
				payload,
				cancellationToken);

			_logger.LogInformation(
				"InApp notification {NotificationId} sent successfully to user {UserId} and group {GroupName}",
				notification.Id,
				notification.UserId,
				authGroupName);

			return new NotificationResult
			{
				Success = true,
				NotificationId = notification.Id,
				Metadata = new Dictionary<string, object>
				{
					["method"] = _options.MethodName,
					["userId"] = notification.UserId,
					["authGroup"] = authGroupName,
					["sentVia"] = "SignalR"
				}
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Failed to send InApp notification {NotificationId} to user {UserId}",
				notification.Id,
				notification.UserId);

			return new NotificationResult
			{
				Success = false,
				NotificationId = notification.Id,
				ErrorMessage = ex.Message
			};
		}
	}

	public Task<bool> ValidateAsync(
		Notification notification,
		CancellationToken cancellationToken = default)
	{
		// ?????? ?? ???? UserId
		if (string.IsNullOrWhiteSpace(notification.UserId))
		{
			_logger.LogWarning(
				"InApp notification {NotificationId} validation failed: UserId is required",
				notification.Id);
			return Task.FromResult(false);
		}

		return Task.FromResult(true);
	}

	private object BuildNotificationPayload(Notification notification)
	{
		var payload = new
		{
			id = notification.Id,
			userId = notification.UserId,
			title = notification.Title,
			message = notification.Message,
			type = notification.Type.ToString(),
			priority = notification.Priority.ToString(),
			createdAt = notification.CreatedAt,
			actionUrl = notification.ActionUrl,
			imageUrl = notification.ImageUrl,
			sound = notification.Sound,
			badgeCount = notification.BadgeCount,
			data = notification.Data
		};

		return payload;
	}
}


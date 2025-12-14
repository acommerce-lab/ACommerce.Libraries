using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Abstractions.Enums;
using ACommerce.Notifications.Abstractions.Models;
using ACommerce.Notifications.Core.Options;
using Microsoft.Extensions.Logging;

namespace ACommerce.Notifications.Core.Services;

/// <summary>
/// ?????? ???????? ?????? ????? ????????? ??? ??????? ????????
/// </summary>
public class NotificationService : INotificationService
{
	private readonly IEnumerable<INotificationChannel> _channels;
	private readonly ILogger<NotificationService> _logger;
	private readonly NotificationOptions _options;

	public NotificationService(
		IEnumerable<INotificationChannel> channels,
		ILogger<NotificationService> logger,
		NotificationOptions options)
	{
		_channels = channels ?? throw new ArgumentNullException(nameof(channels));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_options = options ?? throw new ArgumentNullException(nameof(options));
	}

	public async Task<NotificationResult> SendAsync(
		Notification notification,
		CancellationToken cancellationToken = default)
	{
		if (notification == null)
			throw new ArgumentNullException(nameof(notification));

		_logger.LogInformation(
			"Processing notification {NotificationId} for user {UserId} with {ChannelCount} channels",
			notification.Id,
			notification.UserId,
			notification.Channels.Count);

		// ?????? ?? ?????? ???????
		if (notification.IsExpired)
		{
			_logger.LogWarning(
				"Notification {NotificationId} has expired at {ExpiresAt}",
				notification.Id,
				notification.ExpiresAt);

			return new NotificationResult
			{
				Success = false,
				NotificationId = notification.Id,
				ErrorMessage = "Notification has expired"
			};
		}

		// ?????? ?? ???????
		if (notification.IsScheduled)
		{
			_logger.LogInformation(
				"Notification {NotificationId} is scheduled for {ScheduledAt}",
				notification.Id,
				notification.ScheduledAt);

			return new NotificationResult
			{
				Success = true,
				NotificationId = notification.Id,
				Metadata = new Dictionary<string, object>
				{
					["status"] = "scheduled",
					["scheduledAt"] = notification.ScheduledAt!.Value
				}
			};
		}

		// ?????? ?? ????
		var channelTasks = notification.Channels
			.Where(cd => ShouldProcessChannel(cd, notification))
			.Select(channelDelivery => ProcessChannelAsync(
				notification,
				channelDelivery,
				cancellationToken));

		await Task.WhenAll(channelTasks);

		// ???? ???????
		var result = BuildResult(notification);

		_logger.LogInformation(
			"Notification {NotificationId} processing completed. Success: {Success}, Delivered: {Delivered}/{Total}",
			notification.Id,
			result.Success,
			notification.Channels.Count(c => c.Status == DeliveryStatus.Sent),
			notification.Channels.Count);

		return result;
	}

	public async Task<List<NotificationResult>> SendBatchAsync(
		IEnumerable<Notification> notifications,
		CancellationToken cancellationToken = default)
	{
		if (notifications == null)
			throw new ArgumentNullException(nameof(notifications));

		var notificationList = notifications.ToList();

		_logger.LogInformation(
			"Processing batch of {Count} notifications",
			notificationList.Count);

		var tasks = notificationList.Select(n => SendAsync(n, cancellationToken));
		var results = await Task.WhenAll(tasks);

		var successCount = results.Count(r => r.Success);

		_logger.LogInformation(
			"Batch processing completed. {SuccessCount}/{TotalCount} succeeded",
			successCount,
			notificationList.Count);

		return results.ToList();
	}

	public async Task<NotificationResult> ScheduleAsync(
		Notification notification,
		CancellationToken cancellationToken = default)
	{
		if (notification == null)
			throw new ArgumentNullException(nameof(notification));

		if (!notification.ScheduledAt.HasValue)
			throw new ArgumentException("ScheduledAt must be set for scheduled notifications");

		if (notification.ScheduledAt.Value <= DateTimeOffset.UtcNow)
			throw new ArgumentException("ScheduledAt must be in the future");

		_logger.LogInformation(
			"Scheduled notification {NotificationId} for {ScheduledAt}",
			notification.Id,
			notification.ScheduledAt.Value);

		// ?? ???? ???? Store? ??? ????? ????
		// ??? ???? ??????? ??????
		return new NotificationResult
		{
			Success = true,
			NotificationId = notification.Id,
			Metadata = new Dictionary<string, object>
			{
				["status"] = "scheduled",
				["scheduledAt"] = notification.ScheduledAt.Value
			}
		};
	}

	private bool ShouldProcessChannel(ChannelDelivery channelDelivery, Notification notification)
	{
		// ?????? ?? ??????
		if (channelDelivery.Status == DeliveryStatus.Sent)
		{
			_logger.LogDebug(
				"Skipping channel {Channel} for notification {NotificationId} - already sent",
				channelDelivery.Channel,
				notification.Id);
			return false;
		}

		// ?????? ?? ??????? ????? ????????
		if (channelDelivery.Status == DeliveryStatus.Failed && !channelDelivery.CanRetry())
		{
			_logger.LogDebug(
				"Skipping channel {Channel} for notification {NotificationId} - max retries reached",
				channelDelivery.Channel,
				notification.Id);
			return false;
		}

		// ?????? ?? ???? ????? ????????
		if (channelDelivery.Status == DeliveryStatus.Failed &&
			channelDelivery.NextRetryAt.HasValue &&
			channelDelivery.NextRetryAt.Value > DateTimeOffset.UtcNow)
		{
			_logger.LogDebug(
				"Skipping channel {Channel} for notification {NotificationId} - retry scheduled for {NextRetryAt}",
				channelDelivery.Channel,
				notification.Id,
				channelDelivery.NextRetryAt.Value);
			return false;
		}

		return true;
	}

	private async Task ProcessChannelAsync(
		Notification notification,
		ChannelDelivery channelDelivery,
		CancellationToken cancellationToken)
	{
		var channel = _channels.FirstOrDefault(c => c.Channel == channelDelivery.Channel);

		if (channel == null)
		{
			_logger.LogWarning(
				"No handler registered for channel {Channel}",
				channelDelivery.Channel);

			channelDelivery.RecordFailure($"No handler registered for {channelDelivery.Channel}");
			return;
		}

		try
		{
			channelDelivery.MarkAsSending();

			var attemptNumber = channelDelivery.RetryCount + 1;

			_logger.LogDebug(
				"Sending notification {NotificationId} via {Channel} (attempt {Attempt}/{MaxAttempts})",
				notification.Id,
				channelDelivery.Channel,
				attemptNumber,
				channelDelivery.MaxRetries + 1);

			// ?????? ?? ?????? ??????? ??????
			var isValid = await channel.ValidateAsync(notification, cancellationToken);
			if (!isValid)
			{
				channelDelivery.RecordFailure("Notification validation failed for this channel");

				_logger.LogWarning(
					"Notification {NotificationId} validation failed for channel {Channel}",
					notification.Id,
					channelDelivery.Channel);
				return;
			}

			// ????? ???????
			var result = await channel.SendAsync(notification, cancellationToken);

			if (result.Success)
			{
				channelDelivery.RecordSuccess(result.Metadata);

				_logger.LogInformation(
					"Successfully sent notification {NotificationId} via {Channel}",
					notification.Id,
					channelDelivery.Channel);
			}
			else
			{
				channelDelivery.RecordFailure(result.ErrorMessage ?? "Unknown error");

				_logger.LogWarning(
					"Failed to send notification {NotificationId} via {Channel}: {Error}. " +
					"Retry: {WillRetry} (attempt {Attempt}/{MaxAttempts})",
					notification.Id,
					channelDelivery.Channel,
					result.ErrorMessage,
					channelDelivery.CanRetry(),
					attemptNumber,
					channelDelivery.MaxRetries + 1);
			}
		}
		catch (OperationCanceledException)
		{
			_logger.LogWarning(
				"Notification {NotificationId} sending via {Channel} was cancelled",
				notification.Id,
				channelDelivery.Channel);

			channelDelivery.RecordFailure("Operation was cancelled");
		}
		catch (Exception ex)
		{
			channelDelivery.RecordFailure(ex.Message);

			_logger.LogError(
				ex,
				"Exception sending notification {NotificationId} via {Channel}. " +
				"Will retry: {WillRetry}",
				notification.Id,
				channelDelivery.Channel,
				channelDelivery.CanRetry());
		}
	}

	private NotificationResult BuildResult(Notification notification)
	{
		var sentChannels = notification.Channels
			.Where(c => c.Status == DeliveryStatus.Sent)
			.Select(c => c.Channel.ToString())
			.ToList();

		var failedChannels = notification.Channels
			.Where(c => c.Status == DeliveryStatus.Failed && !c.CanRetry())
			.Select(c => c.Channel.ToString())
			.ToList();

		var pendingRetries = notification.Channels
			.Count(c => c.CanRetry());

		return new NotificationResult
		{
			Success = sentChannels.Any(),
			NotificationId = notification.Id,
			DeliveredChannels = sentChannels,
			FailedChannels = failedChannels,
			Metadata = new Dictionary<string, object>
			{
				["totalChannels"] = notification.Channels.Count,
				["successfulChannels"] = sentChannels.Count,
				["failedChannels"] = failedChannels.Count,
				["pendingRetries"] = pendingRetries,
				["isFullyDelivered"] = notification.IsFullyDelivered,
				["isPartiallyDelivered"] = notification.IsPartiallyDelivered,
				["isCompletelyFailed"] = notification.IsCompletelyFailed
			}
		};
	}
}


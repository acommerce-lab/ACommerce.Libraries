using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace ACommerce.Notifications.Core.Publishers;

/// <summary>
/// ???? ????????? ?? ??????? (??????? ?????????)
/// ???? ????????? ?????? ???? ??????? Message Queue
/// </summary>
public class InMemoryNotificationPublisher : INotificationPublisher
{
	private readonly INotificationService _notificationService;
	private readonly ILogger<InMemoryNotificationPublisher> _logger;

	public InMemoryNotificationPublisher(
		INotificationService notificationService,
		ILogger<InMemoryNotificationPublisher> logger)
	{
		_notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task PublishAsync(
		NotificationEvent notificationEvent,
		CancellationToken cancellationToken = default)
	{
		if (notificationEvent == null)
			throw new ArgumentNullException(nameof(notificationEvent));

		_logger.LogDebug(
			"Publishing notification event {EventId} directly (InMemory mode)",
			notificationEvent.EventId);

		// ????? Event ??? Notification
		var notification = notificationEvent.ToNotification();

		// ????? ?????
		var result = await _notificationService.SendAsync(notification, cancellationToken);

		if (result.Success)
		{
			_logger.LogInformation(
				"Notification event {EventId} published and processed successfully",
				notificationEvent.EventId);
		}
		else
		{
			_logger.LogWarning(
				"Notification event {EventId} published but processing failed: {Error}",
				notificationEvent.EventId,
				result.ErrorMessage);
		}
	}

	public async Task PublishBatchAsync(
		IEnumerable<NotificationEvent> notificationEvents,
		CancellationToken cancellationToken = default)
	{
		if (notificationEvents == null)
			throw new ArgumentNullException(nameof(notificationEvents));

		var events = notificationEvents.ToList();

		_logger.LogDebug(
			"Publishing {Count} notification events directly (InMemory mode)",
			events.Count);

		var notifications = events.Select(e => e.ToNotification()).ToList();

		var results = await _notificationService.SendBatchAsync(notifications, cancellationToken);

		var successCount = results.Count(r => r.Success);

		_logger.LogInformation(
			"Batch of {Count} notification events published. {SuccessCount} processed successfully",
			events.Count,
			successCount);
	}
}


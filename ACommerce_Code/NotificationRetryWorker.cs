using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Core.Options;
using ACommerce.Notifications.Core.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ACommerce.Notifications.Core.Workers;

/// <summary>
/// Background worker ?????? ?????? ????? ????????? ???????
/// ???? ???? ???? ?????? ?? ????????? ???? ????? ????? ??????
/// </summary>
public class NotificationRetryWorker : BackgroundService
{
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly ILogger<NotificationRetryWorker> _logger;
	private readonly NotificationOptions _options;

	public NotificationRetryWorker(
		IServiceScopeFactory scopeFactory,
		NotificationOptions options,
		ILogger<NotificationRetryWorker> logger)
	{
		_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
		_options = options ?? throw new ArgumentNullException(nameof(options));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("Notification Retry Worker started");

		// ???????? ?????? ??? ????? ?????? ??????? ???????? ???????
		await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				if (!_options.Enabled)
				{
					_logger.LogDebug("Notification processing is disabled, worker is idle");
					await Task.Delay(_options.WorkerPollingInterval, stoppingToken);
					continue;
				}

				await ProcessRetriesAsync(stoppingToken);
				await Task.Delay(_options.WorkerPollingInterval, stoppingToken);
			}
			catch (OperationCanceledException)
			{
				_logger.LogInformation("Notification Retry Worker is stopping");
				break;
			}
			catch (Exception ex)
			{
				_logger.LogError(
					ex,
					"Critical error in Notification Retry Worker. " +
					"Will retry after 1 minute");

				await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
			}
		}

		_logger.LogInformation("Notification Retry Worker stopped");
	}

	private async Task ProcessRetriesAsync(CancellationToken cancellationToken)
	{
		using var scope = _scopeFactory.CreateScope();

		// ?????? ?????? ??? Store (???????)
		var store = scope.ServiceProvider.GetService<INotificationStore>();

		if (store == null)
		{
			_logger.LogTrace(
				"No INotificationStore configured, retry worker is idle");
			return;
		}

		var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

		try
		{
			// ??? ????????? ???? ????? ????? ??????
			var notifications = await store.GetNotificationsForRetryAsync(
				_options.WorkerBatchSize,
				cancellationToken);

			if (!notifications.Any())
			{
				_logger.LogTrace("No notifications pending retry");
				return;
			}

			_logger.LogInformation(
				"Found {Count} notifications pending retry",
				notifications.Count);

			var processedCount = 0;
			var successCount = 0;

			foreach (var notification in notifications)
			{
				if (cancellationToken.IsCancellationRequested)
					break;

				try
				{
					_logger.LogDebug(
						"Retrying notification {NotificationId}",
						notification.Id);

					var result = await notificationService.SendAsync(
						notification,
						cancellationToken);

					await store.UpdateNotificationAsync(notification, cancellationToken);

					processedCount++;

					if (result.Success)
					{
						successCount++;

						_logger.LogInformation(
							"Successfully retried notification {NotificationId}",
							notification.Id);
					}
					else
					{
						_logger.LogWarning(
							"Retry failed for notification {NotificationId}: {Error}",
							notification.Id,
							result.ErrorMessage);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(
						ex,
						"Error retrying notification {NotificationId}",
						notification.Id);

					processedCount++;
				}
			}

			_logger.LogInformation(
				"Retry batch completed. Processed: {Processed}, Success: {Success}",
				processedCount,
				successCount);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing retry batch");
		}
	}
}


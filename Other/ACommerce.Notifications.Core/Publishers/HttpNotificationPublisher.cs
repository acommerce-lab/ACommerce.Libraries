using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Abstractions.Exceptions;
using ACommerce.Notifications.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace ACommerce.Notifications.Core.Publishers;

/// <summary>
/// ???? ????????? ??? HTTP (??????? ?? Notification Service ??????)
/// </summary>
public class HttpNotificationPublisher : INotificationPublisher
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<HttpNotificationPublisher> _logger;
	private readonly HttpNotificationPublisherOptions _options;

	public HttpNotificationPublisher(
		HttpClient httpClient,
		HttpNotificationPublisherOptions options,
		ILogger<HttpNotificationPublisher> logger)
	{
		_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		_options = options ?? throw new ArgumentNullException(nameof(options));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));

		if (string.IsNullOrEmpty(_options.ServiceUrl))
			throw new ArgumentException("ServiceUrl is required", nameof(options));

		_httpClient.BaseAddress = new Uri(_options.ServiceUrl);
	}

	public async Task PublishAsync(
		NotificationEvent notificationEvent,
		CancellationToken cancellationToken = default)
	{
		if (notificationEvent == null)
			throw new ArgumentNullException(nameof(notificationEvent));

		_logger.LogDebug(
			"Publishing notification event {EventId} via HTTP to {ServiceUrl}",
			notificationEvent.EventId,
			_options.ServiceUrl);

		try
		{
			var response = await _httpClient.PostAsJsonAsync(
				_options.PublishEndpoint,
				notificationEvent,
				cancellationToken);

			if (response.IsSuccessStatusCode)
			{
				_logger.LogInformation(
					"Notification event {EventId} published successfully via HTTP",
					notificationEvent.EventId);
			}
			else
			{
				var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);

				_logger.LogError(
					"Failed to publish notification event {EventId} via HTTP. " +
					"Status: {StatusCode}, Error: {Error}",
					notificationEvent.EventId,
					response.StatusCode,
					errorContent);

				throw new NotificationException(
					"HTTP_PUBLISH_FAILED",
					$"Failed to publish notification: {response.StatusCode}");
			}
		}
		catch (HttpRequestException ex)
		{
			_logger.LogError(
				ex,
				"HTTP error publishing notification event {EventId}",
				notificationEvent.EventId);

			throw new NotificationException(
				"HTTP_CONNECTION_ERROR",
				"Failed to connect to notification service",
				ex);
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
			"Publishing batch of {Count} notification events via HTTP",
			events.Count);

		try
		{
			var response = await _httpClient.PostAsJsonAsync(
				_options.BatchPublishEndpoint,
				events,
				cancellationToken);

			if (response.IsSuccessStatusCode)
			{
				_logger.LogInformation(
					"Batch of {Count} notification events published successfully via HTTP",
					events.Count);
			}
			else
			{
				var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);

				_logger.LogError(
					"Failed to publish batch via HTTP. Status: {StatusCode}, Error: {Error}",
					response.StatusCode,
					errorContent);

				throw new NotificationException(
					"HTTP_BATCH_PUBLISH_FAILED",
					$"Failed to publish batch: {response.StatusCode}");
			}
		}
		catch (HttpRequestException ex)
		{
			_logger.LogError(ex, "HTTP error publishing batch");

			throw new NotificationException(
				"HTTP_CONNECTION_ERROR",
				"Failed to connect to notification service",
				ex);
		}
	}
}

public class HttpNotificationPublisherOptions
{
	public const string SectionName = "Notifications:HttpPublisher";

	public required string ServiceUrl { get; init; }
	public string PublishEndpoint { get; init; } = "/api/notifications/publish";
	public string BatchPublishEndpoint { get; init; } = "/api/notifications/publish-batch";
	public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
}


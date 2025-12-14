using ACommerce.Messaging.Abstractions.Contracts;
using ACommerce.Messaging.Abstractions.Models;
using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace ACommerce.Notifications.Core.Publishers;

/// <summary>
/// Publisher that uses messaging bus for microservices
/// </summary>
public class MessagingNotificationPublisher(
    IMessagePublisher messagePublisher,
    ILogger<MessagingNotificationPublisher> logger)
    : INotificationPublisher
{
    public async Task PublishAsync(
        NotificationEvent notificationEvent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await messagePublisher.PublishAsync(
                notificationEvent,
                topic: TopicNames.Command("notify", "send"),
                metadata: new MessageMetadata
                {
                    SourceService = "unknown", // Will be set by caller
                    CorrelationId = notificationEvent.EventId.ToString(),
                    Priority = (int)notificationEvent.Priority
                },
                cancellationToken);

            logger.LogInformation(
                "Published notification event {NotificationId} to messaging bus",
                notificationEvent.EventId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to publish notification event {NotificationId}",
                notificationEvent.EventId);
            throw;
        }
    }

    public async Task PublishBatchAsync(
        IEnumerable<NotificationEvent> notificationEvents,
        CancellationToken cancellationToken = default)
    {
        var tasks = notificationEvents.Select(evt =>
            PublishAsync(evt, cancellationToken));

        await Task.WhenAll(tasks);
    }
}
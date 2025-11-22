using ACommerce.Authentication.Abstractions.Contracts;
using ACommerce.Messaging.Abstractions.Contracts;
using ACommerce.Messaging.Abstractions.Models;
using ACommerce.SharedKernel.Abstractions.Entities;
using Microsoft.Extensions.Logging;

namespace ACommerce.Authentication.Messaging.Publishers;

/// <summary>
/// Publishes authentication domain events via messaging bus
/// </summary>
public class MessagingAuthenticationEventPublisher(
    IMessagePublisher messagePublisher,
    ILogger<MessagingAuthenticationEventPublisher> logger) : IAuthenticationEventPublisher
{
    public async Task PublishAsync<TEvent>(
        TEvent domainEvent,
        CancellationToken cancellationToken = default)
        where TEvent : class, IDomainEvent
    {
        try
        {
            // Convert event name: TwoFactorSucceededEvent → twofactorsucceeded
            var eventName = domainEvent.GetType().Name
                .Replace("Event", "")
                .ToLowerInvariant();

            var topic = $"auth.events.authentication.{eventName}";

            logger.LogDebug(
                "[Auth Event Publisher] 📤 Publishing {EventType} to topic '{Topic}'",
                domainEvent.GetType().Name,
                topic);

            await messagePublisher.PublishAsync(
                domainEvent,
                topic: topic,
                metadata: new MessageMetadata
                {
                    SourceService = "auth",
                    CorrelationId = Guid.NewGuid().ToString()
                },
                cancellationToken);

            logger.LogInformation(
                "[Auth Event Publisher] ✅ Published {EventType} to topic '{Topic}'",
                domainEvent.GetType().Name,
                topic);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[Auth Event Publisher] 💥 Failed to publish {EventType}",
                domainEvent.GetType().Name);
            throw;
        }
    }
}
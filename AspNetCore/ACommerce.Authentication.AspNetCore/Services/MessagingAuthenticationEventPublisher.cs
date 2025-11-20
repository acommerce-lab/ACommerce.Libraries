using ACommerce.Authentication.Abstractions.Contracts;
using ACommerce.Messaging.Abstractions.Contracts;
using ACommerce.Messaging.Abstractions.Models;
using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Authentication.AspNetCore.Services;

public class MessagingAuthenticationEventPublisher(IMessagePublisher messagePublisher) : IAuthenticationEventPublisher
{
    public async Task PublishAsync<TEvent>(
        TEvent domainEvent,
        CancellationToken cancellationToken = default)
        where TEvent : class, IDomainEvent // <-- Add 'class' constraint to TEvent
    {
        var topic = TopicNames.Event(
            service: "auth",
            entity: "authentication",
            action: domainEvent.GetType().Name.Replace("Event", "").ToLowerInvariant()
        );

        await messagePublisher.PublishAsync(
            domainEvent,
            topic: topic,
            metadata: new MessageMetadata
            {
                SourceService = "auth",
                CorrelationId = Guid.NewGuid().ToString()
            },
            cancellationToken);
    }
}

// ✅ Null implementation (when messaging is not available)
public class NullAuthenticationEventPublisher : IAuthenticationEventPublisher
{
    public Task PublishAsync<TEvent>(
        TEvent domainEvent,
        CancellationToken cancellationToken = default)
        where TEvent : class, IDomainEvent
    {
        // Do nothing - for environments without messaging
        return Task.CompletedTask;
    }
}
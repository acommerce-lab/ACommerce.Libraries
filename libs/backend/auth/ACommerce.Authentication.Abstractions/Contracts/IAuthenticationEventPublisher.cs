using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Authentication.Abstractions.Contracts;

/// <summary>
/// Publisher for authentication events
/// </summary>
public interface IAuthenticationEventPublisher
{
    Task PublishAsync<TEvent>(
        TEvent domainEvent,
        CancellationToken cancellationToken = default)
        where TEvent : class, IDomainEvent;
}

using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Authentication.Abstractions.Events;

/// <summary>
/// Authentication failed
/// </summary>
public record AuthenticationFailedEvent : IDomainEvent
{
    public required string Identifier { get; init; }  // Username, Phone, etc.
    public required string Provider { get; init; }
    public required string Reason { get; init; }
    public required string IpAddress { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}

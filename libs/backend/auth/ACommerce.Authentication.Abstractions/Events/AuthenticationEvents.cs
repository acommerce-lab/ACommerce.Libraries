using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Authentication.Abstractions.Events;

/// <summary>
/// User authenticated successfully
/// </summary>
public record UserAuthenticatedEvent : IDomainEvent
{
    public required string UserId { get; init; }
    public required string Provider { get; init; }  // "JWT", "Nafath", etc.
    public required string IpAddress { get; init; }
    public string? DeviceInfo { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}

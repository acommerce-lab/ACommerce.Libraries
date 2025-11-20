using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Authentication.Abstractions.Events;

/// <summary>
/// User logged out
/// </summary>
public record UserLoggedOutEvent : IDomainEvent
{
    public required string UserId { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
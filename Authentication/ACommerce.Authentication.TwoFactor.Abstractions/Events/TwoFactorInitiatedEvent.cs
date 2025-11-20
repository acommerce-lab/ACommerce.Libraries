using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Authentication.TwoFactor.Abstractions.Events;

/// <summary>
/// Two-factor authentication initiated
/// </summary>
public record TwoFactorInitiatedEvent : IDomainEvent
{
    public required string TransactionId { get; init; }
    public required string Identifier { get; init; }  // Phone number, NationalId, etc.
    public required string Provider { get; init; }  // "Nafath", "SMS", etc.
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}

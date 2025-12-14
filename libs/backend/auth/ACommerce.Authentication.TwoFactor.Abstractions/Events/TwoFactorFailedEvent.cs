using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Authentication.TwoFactor.Abstractions.Events;

/// <summary>
/// Two-factor authentication failed
/// </summary>
public record TwoFactorFailedEvent : IDomainEvent
{
    public required string TransactionId { get; init; }
    public required string Identifier { get; init; }
    public required string Provider { get; init; }
    public required string Reason { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}

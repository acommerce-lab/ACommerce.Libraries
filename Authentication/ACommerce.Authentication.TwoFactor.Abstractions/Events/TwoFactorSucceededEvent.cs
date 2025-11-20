using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Authentication.TwoFactor.Abstractions.Events;

/// <summary>
/// Two-factor authentication succeeded
/// </summary>
public record TwoFactorSucceededEvent : IDomainEvent
{
    public required string TransactionId { get; init; }
    public required string Identifier { get; init; }
    public required string Provider { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}

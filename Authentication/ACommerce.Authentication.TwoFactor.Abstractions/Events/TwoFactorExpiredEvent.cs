using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Authentication.TwoFactor.Abstractions.Events;

/// <summary>
/// Two-factor authentication expired
/// </summary>
public record TwoFactorExpiredEvent : IDomainEvent
{
    public required string TransactionId { get; init; }
    public required string Identifier { get; init; }
    public required string Provider { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
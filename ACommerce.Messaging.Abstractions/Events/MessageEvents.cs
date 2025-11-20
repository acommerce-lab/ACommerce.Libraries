using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Messaging.Abstractions.Events;

/// <summary>
/// Message published event
/// </summary>
public record MessagePublishedEvent : IDomainEvent
{
    public required string MessageId { get; init; }
    public required string Topic { get; init; }
    public required string SourceService { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Message received event
/// </summary>
public record MessageReceivedEvent : IDomainEvent
{
    public required string MessageId { get; init; }
    public required string Topic { get; init; }
    public required string TargetService { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Message failed event
/// </summary>
public record MessageFailedEvent : IDomainEvent
{
    public required string MessageId { get; init; }
    public required string Topic { get; init; }
    public required string Error { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
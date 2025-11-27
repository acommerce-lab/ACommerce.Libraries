namespace ACommerce.Profiles.Core.Events;

/// <summary>
/// Event raised when a profile is created
/// </summary>
public record ProfileCreatedEvent
{
    public required string UserId { get; init; }
    public required Guid ProfileId { get; init; }
    public required string DisplayName { get; init; }
    public required string ChatIdentifier { get; init; }
    public required List<ContactPointCreatedData> ContactPoints { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}

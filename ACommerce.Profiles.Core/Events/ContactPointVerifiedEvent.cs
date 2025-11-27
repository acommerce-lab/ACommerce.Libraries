namespace ACommerce.Profiles.Core.Events;

/// <summary>
/// Event raised when a contact point is verified
/// </summary>
public record ContactPointVerifiedEvent
{
    public required string UserId { get; init; }
    public required ContactPointType Type { get; init; }
    public required string Value { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}

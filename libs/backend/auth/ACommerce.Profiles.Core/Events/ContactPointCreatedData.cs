namespace ACommerce.Profiles.Core.Events;

/// <summary>
/// Contact point data in events
/// </summary>
public record ContactPointCreatedData
{
    public required ContactPointType Type { get; init; }
    public required string Value { get; init; }
    public bool IsVerified { get; init; }
    public bool IsPrimary { get; init; }
}

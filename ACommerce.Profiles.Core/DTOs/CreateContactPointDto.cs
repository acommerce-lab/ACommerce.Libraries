namespace ACommerce.Profiles.Core.DTOs;

/// <summary>
/// DTO for creating a contact point
/// </summary>
public record CreateContactPointDto
{
    public required ContactPointType Type { get; init; }
    public required string Value { get; init; }
    public bool IsPrimary { get; init; }
    public bool IsVerified { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}

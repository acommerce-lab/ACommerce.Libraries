namespace ACommerce.Profiles.Core.DTOs;

/// <summary>
/// Contact point DTO
/// </summary>
public record ContactPointDto
{
    public Guid Id { get; init; }
    public ContactPointType Type { get; init; }
    public string Value { get; init; } = default!;
    public bool IsVerified { get; init; }
    public bool IsPrimary { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = new();
}

namespace ACommerce.Profiles.Core.DTOs;

/// <summary>
/// DTO for creating a user profile
/// </summary>
public record CreateProfileDto
{
    public required string UserId { get; init; }
    public required string DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
    public string? Bio { get; init; }
    public List<CreateContactPointDto>? ContactPoints { get; init; }
}

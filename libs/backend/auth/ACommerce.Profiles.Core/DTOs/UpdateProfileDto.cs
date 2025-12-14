namespace ACommerce.Profiles.Core.DTOs;

/// <summary>
/// DTO for updating a profile
/// </summary>
public record UpdateProfileDto
{
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
    public string? Bio { get; init; }
}

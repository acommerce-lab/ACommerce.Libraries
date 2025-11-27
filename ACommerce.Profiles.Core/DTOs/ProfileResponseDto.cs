namespace ACommerce.Profiles.Core.DTOs;

/// <summary>
/// Profile response DTO
/// </summary>
public record ProfileResponseDto
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public string? AvatarUrl { get; init; }
    public string? Bio { get; init; }
    public string ChatIdentifier { get; init; } = default!;
    public List<ContactPointDto> ContactPoints { get; init; } = new();
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

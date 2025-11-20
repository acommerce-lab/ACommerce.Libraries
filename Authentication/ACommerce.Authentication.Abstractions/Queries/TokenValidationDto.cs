namespace ACommerce.Authentication.Abstractions.Queries;

/// <summary>
/// Token validation response
/// </summary>
public record TokenValidationDto
{
    public bool IsValid { get; init; }
    public string? UserId { get; init; }
    public string? Username { get; init; }
    public Dictionary<string, string>? Claims { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}
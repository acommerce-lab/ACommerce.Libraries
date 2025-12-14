namespace ACommerce.Authentication.Abstractions.Queries;

/// <summary>
/// User data response
/// </summary>
public record UserDto
{
    public required string Id { get; init; }
    public required string Username { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Name { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}

// Models/UpdateUserRequest.cs

namespace ACommerce.Authentication.Users.Abstractions.Models;

// Models/UpdateUserRequest.cs
public record UpdateUserRequest
{
	public string? Username { get; init; }
	public string? Email { get; init; }
	public string? PhoneNumber { get; init; }
	public bool? IsActive { get; init; }
	public bool? TwoFactorEnabled { get; init; }
	public Dictionary<string, string>? Metadata { get; init; }
}


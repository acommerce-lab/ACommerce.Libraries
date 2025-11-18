// Models/CreateUserRequest.cs
namespace ACommerce.Authentication.Users.Abstractions.Models;

public record CreateUserRequest
{
	public required string Username { get; init; }
	public required string Email { get; init; }
	public string? PhoneNumber { get; init; }
	public required string Password { get; init; }
	public bool TwoFactorEnabled { get; init; } = false;
	public List<string>? Roles { get; init; }
	public Dictionary<string, string>? Metadata { get; init; }
	public bool RequireEmailConfirmation { get; set; }
}


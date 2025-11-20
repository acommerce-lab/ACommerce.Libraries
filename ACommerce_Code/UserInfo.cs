// Models/UserInfo.cs
namespace ACommerce.Authentication.Users.Abstractions.Models;

// Models/UserInfo.cs
/// <summary>
/// ??????? ???????? - DTO ???
/// </summary>
public record UserInfo
{
	public required string UserId { get; init; }
	public required string Username { get; init; }
	public required string Email { get; init; }
	public string? PhoneNumber { get; init; }
	public bool EmailVerified { get; init; }
	public bool PhoneNumberVerified { get; init; }
	public bool TwoFactorEnabled { get; init; }
	public bool IsActive { get; init; }
	public bool IsLocked { get; init; }
	public DateTimeOffset? LockoutEnd { get; init; }
	public DateTimeOffset? LastLoginAt { get; init; }
	public List<string> Roles { get; init; } = new();
	public Dictionary<string, string> Claims { get; init; } = new();
	public Dictionary<string, string> Metadata { get; init; } = new();
}


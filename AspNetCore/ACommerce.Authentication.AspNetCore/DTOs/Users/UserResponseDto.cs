// DTOs/Users/UserResponseDto.cs
namespace ACommerce.Authentication.AspNetCore.DTOs.Users;

public class UserResponseDto
{
	public string UserId { get; set; } = string.Empty;
	public string Username { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string? PhoneNumber { get; set; }
	public bool EmailVerified { get; set; }
	public bool PhoneNumberVerified { get; set; }
	public bool TwoFactorEnabled { get; set; }
	public bool IsActive { get; set; }
	public bool IsLocked { get; set; }
	public DateTimeOffset? LockoutEnd { get; set; }
	public DateTimeOffset? LastLoginAt { get; set; }
	public List<string> Roles { get; set; } = new();
	public Dictionary<string, string> Metadata { get; set; } = new();
}


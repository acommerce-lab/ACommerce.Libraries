// DTOs/Users/CreateUserDto.cs
namespace ACommerce.Authentication.AspNetCore.DTOs.Users;

/// <summary>
/// DTO ?????? ?????? ????
/// </summary>
public class CreateUserDto
{
	public required string Username { get; init; }
	public required string Email { get; init; }
	public string? PhoneNumber { get; init; }
	public required string Password { get; init; }
	public required string ConfirmPassword { get; init; }
	public bool TwoFactorEnabled { get; init; } = false;
	public List<string>? Roles { get; init; }
	public Dictionary<string, string>? Metadata { get; init; }
}


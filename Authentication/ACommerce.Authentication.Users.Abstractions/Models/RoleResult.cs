// Models/RoleResult.cs
namespace ACommerce.Authentication.Users.Abstractions.Models;

// Models/RoleResult.cs
public record RoleResult
{
	public required bool Success { get; init; }
	public RoleInfo? Role { get; init; }
	public RoleError? Error { get; init; }
}


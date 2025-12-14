// Models/UserResult.cs
namespace ACommerce.Authentication.Users.Abstractions.Models;

// Models/UserResult.cs
public record UserResult
{
	public required bool Success { get; init; }
	public UserInfo? User { get; init; }
	public UserError? Error { get; init; }
}


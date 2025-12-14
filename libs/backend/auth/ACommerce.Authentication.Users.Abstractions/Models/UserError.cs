// Models/UserError.cs
namespace ACommerce.Authentication.Users.Abstractions.Models;

public record UserError
{
	public required string Code { get; init; }
	public required string Message { get; init; }
	public string? Details { get; init; }
}


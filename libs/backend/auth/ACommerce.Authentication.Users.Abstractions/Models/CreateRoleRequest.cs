// Models/CreateRoleRequest.cs
namespace ACommerce.Authentication.Users.Abstractions.Models;

// Models/CreateRoleRequest.cs
public record CreateRoleRequest
{
	public required string Name { get; init; }
	public string? Description { get; init; }
	public Dictionary<string, string>? Metadata { get; init; }
}


// Models/UpdateRoleRequest.cs
namespace ACommerce.Authentication.Users.Abstractions.Models;

// Models/UpdateRoleRequest.cs
public record UpdateRoleRequest
{
	public string? Name { get; init; }
	public string? Description { get; init; }
	public bool? IsActive { get; init; }
	public Dictionary<string, string>? Metadata { get; init; }
}


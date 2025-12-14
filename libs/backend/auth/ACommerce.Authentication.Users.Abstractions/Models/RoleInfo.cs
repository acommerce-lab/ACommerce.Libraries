// Models/RoleInfo.cs
namespace ACommerce.Authentication.Users.Abstractions.Models;

// Models/RoleInfo.cs
/// <summary>
/// ??????? ????? - DTO ???
/// </summary>
public record RoleInfo
{
	public required string RoleId { get; init; }
	public required string RoleName { get; init; }
	public string? Description { get; init; }
	public bool IsActive { get; init; }
	public Dictionary<string, string> Claims { get; init; } = new();
	public Dictionary<string, string> Metadata { get; init; } = new();
}


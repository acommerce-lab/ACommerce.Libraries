// Entities/RoleClaim.cs
using SharedKernel;
using SharedKernel.Abstractions.Entities;

namespace ACommerce.Authentication.Users.Abstractions.Entities;

/// <summary>
/// Claims ????? (?????????)
/// </summary>
public class RoleClaim : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// ???? ?????
	/// </summary>
	public Guid RoleId { get; set; }
	public Role? Role { get; set; }

	/// <summary>
	/// ??? ??? Claim (????? "Permission")
	/// </summary>
	public required string ClaimType { get; init; }

	/// <summary>
	/// ???? ??? Claim (???: "products.create")
	/// </summary>
	public required string ClaimValue { get; set; }

	/// <summary>
	/// ??????? ??????
	/// </summary>
	public Dictionary<string, string> Metadata { get; init; } = new();
}


// Entities/UserClaim.cs
using System.ComponentModel.DataAnnotations.Schema;
using SharedKernel;

namespace ACommerce.Authentication.Users.Abstractions.Entities;

/// <summary>
/// Claims ????????
/// </summary>
public class UserClaim : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// ???? ????????
	/// </summary>
	public Guid UserId { get; set; }
	public User? User { get; set; }

	/// <summary>
	/// ??? ??? Claim
	/// </summary>
	public required string ClaimType { get; init; }

	/// <summary>
	/// ???? ??? Claim
	/// </summary>
	public required string ClaimValue { get; set; }

	/// <summary>
	/// ??????? ??????
	/// </summary>
	[NotMapped]
	public Dictionary<string, string> Metadata { get; init; } = new();
}


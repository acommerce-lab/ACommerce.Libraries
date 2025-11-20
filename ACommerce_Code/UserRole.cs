// Entities/UserRole.cs
using SharedKernel;

namespace ACommerce.Authentication.Users.Abstractions.Entities;

/// <summary>
/// ??? ???????? ??????
/// </summary>
public class UserRole : IBaseEntity
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
	/// ???? ?????
	/// </summary>
	public Guid RoleId { get; set; }
	public Role? Role { get; set; }

	/// <summary>
	/// ????? ?????? ???????? (???????)
	/// </summary>
	public DateTimeOffset? ExpiresAt { get; set; }

	/// <summary>
	/// ??????? ??????
	/// </summary>
	public Dictionary<string, string> Metadata { get; init; } = new();
}


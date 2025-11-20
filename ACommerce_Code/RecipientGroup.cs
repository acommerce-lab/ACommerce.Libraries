using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Notifications.Recipients.Entities;

/// <summary>
/// ?????? ?? ?????????
/// </summary>
public class RecipientGroup : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// ??? ????????
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// ??? ????????
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// ????????? ?? ????????
	/// </summary>
	public List<UserRecipient> Members { get; init; } = new();

	/// <summary>
	/// ??????? ??????
	/// </summary>
	public Dictionary<string, string> Metadata { get; init; } = new();

	/// <summary>
	/// ?? ???????? ?????
	/// </summary>
	public bool IsActive { get; set; } = true;
}


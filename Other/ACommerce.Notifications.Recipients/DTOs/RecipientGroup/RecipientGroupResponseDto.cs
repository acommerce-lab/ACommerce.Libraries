namespace ACommerce.Notifications.Recipients.DTOs.RecipientGroup;

/// <summary>
/// DTO ???? ?????? ???????
/// </summary>
public class RecipientGroupResponseDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	public bool IsActive { get; set; }
	public int MemberCount { get; set; }
	public Dictionary<string, string> Metadata { get; set; } = new();
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}


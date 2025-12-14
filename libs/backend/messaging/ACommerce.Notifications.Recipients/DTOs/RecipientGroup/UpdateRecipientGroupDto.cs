namespace ACommerce.Notifications.Recipients.DTOs.RecipientGroup;

/// <summary>
/// DTO ?????? ?????? ???????
/// </summary>
public class UpdateRecipientGroupDto
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public bool? IsActive { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}


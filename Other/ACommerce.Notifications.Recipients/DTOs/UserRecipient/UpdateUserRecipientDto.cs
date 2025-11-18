namespace ACommerce.Notifications.Recipients.DTOs.UserRecipient;

/// <summary>
/// DTO ?????? ?????
/// </summary>
public class UpdateUserRecipientDto
{
	public string? FullName { get; set; }
	public string? PreferredLanguage { get; set; }
	public string? TimeZone { get; set; }
	public bool? IsActive { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}


using ACommerce.Notifications.Recipients.DTOs.ContactPoint;
using ACommerce.Notifications.Recipients.DTOs.RecipientGroup;

namespace ACommerce.Notifications.Recipients.DTOs.UserRecipient;

/// <summary>
/// DTO ???? ?????
/// </summary>
public class UserRecipientResponseDto
{
	public Guid Id { get; set; }
	public string UserId { get; set; } = string.Empty;
	public string? FullName { get; set; }
	public string PreferredLanguage { get; set; } = "en";
	public string TimeZone { get; set; } = "UTC";
	public bool IsActive { get; set; }
	public List<ContactPointResponseDto> ContactPoints { get; set; } = new();
	public List<RecipientGroupResponseDto> Groups { get; set; } = new();
	public Dictionary<string, string> Metadata { get; set; } = new();
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}


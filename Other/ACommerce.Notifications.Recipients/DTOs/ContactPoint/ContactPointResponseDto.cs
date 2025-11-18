using ACommerce.Notifications.Recipients.Enums;

namespace ACommerce.Notifications.Recipients.DTOs.ContactPoint;

/// <summary>
/// DTO ???? ???? ?????
/// </summary>
public class ContactPointResponseDto
{
	public Guid Id { get; set; }
	public string UserId { get; set; } = string.Empty;
	public ContactPointType Type { get; set; }
	public string Value { get; set; } = string.Empty;
	public bool IsVerified { get; set; }
	public bool IsPrimary { get; set; }
	public Dictionary<string, string> Metadata { get; set; } = new();
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}


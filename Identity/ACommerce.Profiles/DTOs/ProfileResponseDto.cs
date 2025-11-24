using ACommerce.Profiles.Enums;

namespace ACommerce.Profiles.DTOs;

public class ProfileResponseDto
{
	public Guid Id { get; set; }
	public string UserId { get; set; } = string.Empty;
	public ProfileType Type { get; set; }
	public string? FullName { get; set; }
	public string? BusinessName { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Email { get; set; }
	public string? Avatar { get; set; }
	public string? Address { get; set; }
	public string? City { get; set; }
	public string? Country { get; set; }
	public string? PostalCode { get; set; }
	public bool IsActive { get; set; }
	public bool IsVerified { get; set; }
	public DateTime? VerifiedAt { get; set; }
	public DateTime CreatedAt { get; set; }
	public Dictionary<string, string> Metadata { get; set; } = new();
}

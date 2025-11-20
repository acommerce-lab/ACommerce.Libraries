namespace ACommerce.Notifications.Recipients.DTOs.ContactPoint;

/// <summary>
/// DTO ??????? ?????? ????? ?????
/// </summary>
public class PartialUpdateContactPointDto
{
	public string? Value { get; set; }
	public bool? IsPrimary { get; set; }
	public bool? IsVerified { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}


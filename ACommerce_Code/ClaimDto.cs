// DTOs/Claims/ClaimDto.cs
namespace ACommerce.Authentication.AspNetCore.DTOs.Claims;

/// <summary>
/// DTO ??? Claim
/// </summary>
public class ClaimDto
{
	/// <summary>
	/// ??? ??? Claim
	/// </summary>
	public required string Type { get; init; }

	/// <summary>
	/// ???? ??? Claim
	/// </summary>
	public required string Value { get; init; }
}


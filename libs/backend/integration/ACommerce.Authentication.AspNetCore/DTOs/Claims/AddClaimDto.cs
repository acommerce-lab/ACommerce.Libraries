// DTOs/Claims/AddClaimDto.cs
namespace ACommerce.Authentication.AspNetCore.DTOs.Claims;

// DTOs/Claims/AddClaimDto.cs
/// <summary>
/// DTO ?????? Claim
/// </summary>
public class AddClaimDto
{
	/// <summary>
	/// ??? ??? Claim
	/// </summary>
	public required string ClaimType { get; init; }

	/// <summary>
	/// ???? ??? Claim
	/// </summary>
	public required string ClaimValue { get; init; }
}


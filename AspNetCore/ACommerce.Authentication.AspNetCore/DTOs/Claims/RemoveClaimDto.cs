// DTOs/Claims/RemoveClaimDto.cs
namespace ACommerce.Authentication.AspNetCore.DTOs.Claims;

// DTOs/Claims/RemoveClaimDto.cs
/// <summary>
/// DTO ?????? Claim
/// </summary>
public class RemoveClaimDto
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


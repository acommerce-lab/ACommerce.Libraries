// DTOs/Roles/CreateRoleDto.cs
using ACommerce.Authentication.AspNetCore.DTOs.Claims;

namespace ACommerce.Authentication.AspNetCore.DTOs.Roles;

/// <summary>
/// DTO ?????? ??? ????
/// </summary>
public class CreateRoleDto
{
	/// <summary>
	/// ??? ?????
	/// </summary>
	public required string Name { get; init; }

	/// <summary>
	/// ??? ?????
	/// </summary>
	public string? Description { get; init; }

	/// <summary>
	/// ????????? ???????
	/// </summary>
	public List<ClaimDto>? Claims { get; init; }

	/// <summary>
	/// ??????? ??????
	/// </summary>
	public Dictionary<string, string>? Metadata { get; init; }
}


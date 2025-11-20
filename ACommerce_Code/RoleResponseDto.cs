// DTOs/Roles/RoleResponseDto.cs
using ACommerce.Authentication.AspNetCore.DTOs.Claims;

namespace ACommerce.Authentication.AspNetCore.DTOs.Roles;

// DTOs/Roles/RoleResponseDto.cs
/// <summary>
/// DTO ???? ?????? ?????
/// </summary>
public class RoleResponseDto
{
	public string RoleId { get; set; } = string.Empty;
	public string RoleName { get; set; } = string.Empty;
	public string? Description { get; set; }
	public bool IsActive { get; set; }
	public List<ClaimDto> Claims { get; set; } = new();
	public Dictionary<string, string> Metadata { get; set; } = new();
}


// Controllers/RolesController.cs (?????? ???????)
using ACommerce.Authentication.AspNetCore.DTOs.Roles;
using ACommerce.Authentication.AspNetCore.DTOs.Claims;
using ACommerce.Authentication.Users.Abstractions;
using ACommerce.Authentication.Users.Abstractions.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ACommerce.Authentication.AspNetCore.Controllers;

/// <summary>
/// Controller ?????? ???????
/// </summary>
[ApiController]
[Route("api/roles")]
[Authorize(Roles = "Admin")]
public class RolesController : ControllerBase
{
	private readonly IRoleProvider _roleProvider;
	private readonly IClaimProvider _claimProvider;
	private readonly ILogger<RolesController> _logger;

	public RolesController(
		IRoleProvider roleProvider,
		IClaimProvider claimProvider,
		ILogger<RolesController> logger)
	{
		_roleProvider = roleProvider ?? throw new ArgumentNullException(nameof(roleProvider));
		_claimProvider = claimProvider ?? throw new ArgumentNullException(nameof(claimProvider));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// ????? ??? ????
	/// </summary>
	[HttpPost]
	public async Task<IActionResult> CreateRole(
		[FromBody] CreateRoleDto dto,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Creating new role: {RoleName}", dto.Name);

			var request = new CreateRoleRequest
			{
				Name = dto.Name,
				Description = dto.Description,
				Metadata = dto.Metadata
			};

			var result = await _roleProvider.CreateRoleAsync(request, cancellationToken);

			if (!result.Success)
			{
				return BadRequest(new
				{
					error = result.Error?.Code,
					message = result.Error?.Message,
					details = result.Error?.Details
				});
			}

			// ????? Claims ?? ????
			if (dto.Claims?.Any() == true)
			{
				foreach (var claim in dto.Claims)
				{
					await _claimProvider.AddRoleClaimAsync(
						dto.Name,
						claim.Type,
						claim.Value,
						cancellationToken);
				}
			}

			return CreatedAtAction(
				nameof(GetRoleByName),
				new { roleName = result.Role!.RoleName },
				MapToRoleResponseDto(result.Role));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating role");
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ??? ??? ???????
	/// </summary>
	[HttpGet("{roleId}")]
	public async Task<IActionResult> GetRole(
		string roleId,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var role = await _roleProvider.GetRoleByIdAsync(roleId, cancellationToken);

			if (role == null)
			{
				return NotFound(new
				{
					error = "ROLE_NOT_FOUND",
					message = "Role not found"
				});
			}

			return Ok(MapToRoleResponseDto(role));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting role: {RoleId}", roleId);
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ??? ??? ??????
	/// </summary>
	[HttpGet("by-name/{roleName}")]
	public async Task<IActionResult> GetRoleByName(
		string roleName,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var role = await _roleProvider.GetRoleByNameAsync(roleName, cancellationToken);

			if (role == null)
			{
				return NotFound(new
				{
					error = "ROLE_NOT_FOUND",
					message = "Role not found"
				});
			}

			return Ok(MapToRoleResponseDto(role));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting role by name: {RoleName}", roleName);
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ??? ???? ???????
	/// </summary>
	[HttpGet]
	public async Task<IActionResult> GetAllRoles(CancellationToken cancellationToken = default)
	{
		try
		{
			var roles = await _roleProvider.GetAllRolesAsync(cancellationToken);

			var response = roles.Select(MapToRoleResponseDto).ToList();

			return Ok(response);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting all roles");
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ????? ???
	/// </summary>
	[HttpPut("{roleId}")]
	public async Task<IActionResult> UpdateRole(
		string roleId,
		[FromBody] UpdateRoleDto dto,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Updating role: {RoleId}", roleId);

			var request = new UpdateRoleRequest
			{
				Name = dto.Name,
				Description = dto.Description,
				IsActive = dto.IsActive,
				Metadata = dto.Metadata
			};

			var result = await _roleProvider.UpdateRoleAsync(roleId, request, cancellationToken);

			if (!result.Success)
			{
				return BadRequest(new
				{
					error = result.Error?.Code,
					message = result.Error?.Message
				});
			}

			return await GetRole(roleId, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating role: {RoleId}", roleId);
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ??? ???
	/// </summary>
	[HttpDelete("{roleId}")]
	public async Task<IActionResult> DeleteRole(
		string roleId,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Deleting role: {RoleId}", roleId);

			var success = await _roleProvider.DeleteRoleAsync(roleId, cancellationToken);

			if (!success)
			{
				return NotFound(new
				{
					error = "ROLE_NOT_FOUND",
					message = "Role not found or already deleted"
				});
			}

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting role: {RoleId}", roleId);
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ????? ?????? ????
	/// </summary>
	[HttpPost("{roleName}/users")]
	public async Task<IActionResult> AddUserToRole(
		string roleName,
		[FromBody] AddUserToRoleDto dto,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation(
				"Adding user {UserId} to role {RoleName}",
				dto.UserId,
				roleName);

			var success = await _roleProvider.AddUserToRoleAsync(
				dto.UserId,
				roleName,
				cancellationToken);

			if (!success)
			{
				return BadRequest(new
				{
					error = "ADD_USER_FAILED",
					message = "Failed to add user to role"
				});
			}

			return Ok(new
			{
				success = true,
				message = "User added to role successfully"
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding user to role");
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ????? ?????? ?? ???
	/// </summary>
	[HttpDelete("{roleName}/users/{userId}")]
	public async Task<IActionResult> RemoveUserFromRole(
		string roleName,
		string userId,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation(
				"Removing user {UserId} from role {RoleName}",
				userId,
				roleName);

			var success = await _roleProvider.RemoveUserFromRoleAsync(
				userId,
				roleName,
				cancellationToken);

			if (!success)
			{
				return BadRequest(new
				{
					error = "REMOVE_USER_FAILED",
					message = "Failed to remove user from role"
				});
			}

			return Ok(new
			{
				success = true,
				message = "User removed from role successfully"
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error removing user from role");
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ????? ?????? ????
	/// </summary>
	[HttpPost("{roleName}/claims")]
	public async Task<IActionResult> AddClaimToRole(
		string roleName,
		[FromBody] AddClaimDto dto,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation(
				"Adding claim {Type}={Value} to role {RoleName}",
				dto.ClaimType,
				dto.ClaimValue,
				roleName);

			var success = await _claimProvider.AddRoleClaimAsync(
				roleName,
				dto.ClaimType,
				dto.ClaimValue,
				cancellationToken);

			if (!success)
			{
				return BadRequest(new
				{
					error = "ADD_CLAIM_FAILED",
					message = "Failed to add claim to role"
				});
			}

			return Ok(new
			{
				success = true,
				message = "Claim added to role successfully"
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding claim to role");
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ????? ?????? ?? ???
	/// </summary>
	[HttpDelete("{roleName}/claims")]
	public async Task<IActionResult> RemoveClaimFromRole(
		string roleName,
		[FromBody] RemoveClaimDto dto,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation(
				"Removing claim {Type}={Value} from role {RoleName}",
				dto.ClaimType,
				dto.ClaimValue,
				roleName);

			var success = await _claimProvider.RemoveRoleClaimAsync(
				roleName,
				dto.ClaimType,
				dto.ClaimValue,
				cancellationToken);

			if (!success)
			{
				return BadRequest(new
				{
					error = "REMOVE_CLAIM_FAILED",
					message = "Failed to remove claim from role"
				});
			}

			return Ok(new
			{
				success = true,
				message = "Claim removed from role successfully"
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error removing claim from role");
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ??? ??????? ?????
	/// </summary>
	[HttpGet("{roleName}/claims")]
	public async Task<IActionResult> GetRoleClaims(
		string roleName,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var claims = await _claimProvider.GetRoleClaimsAsync(roleName, cancellationToken);

			return Ok(new
			{
				roleName,
				claims
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting role claims: {RoleName}", roleName);
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ??? ??????? ?????
	/// </summary>
	[HttpGet("{roleName}/users")]
	public async Task<IActionResult> GetRoleUsers(
		string roleName,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var users = await _roleProvider.GetUsersInRoleAsync(roleName, cancellationToken);

			return Ok(new
			{
				roleName,
				userCount = users.Count,
				users = users.Select(u => new
				{
					userId = u.UserId,
					username = u.Username,
					email = u.Email
				}).ToList()
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting role users: {RoleName}", roleName);
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	// Helper method
	private RoleResponseDto MapToRoleResponseDto(RoleInfo role)
	{
		return new RoleResponseDto
		{
			RoleId = role.RoleId,
			RoleName = role.RoleName,
			Description = role.Description,
			IsActive = role.IsActive,
			Claims = role.Claims.Select(c => new ClaimDto
			{
				Type = c.Key,
				Value = c.Value
			}).ToList(),
			Metadata = role.Metadata
		};
	}
}


// Controllers/UsersController.cs
using ACommerce.Authentication.AspNetCore.DTOs.Users;
using ACommerce.Authentication.AspNetCore.DTOs.Claims;
using ACommerce.Authentication.Users.Abstractions;
using ACommerce.Authentication.Users.Abstractions.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ACommerce.Authentication.AspNetCore.Controllers;

/// <summary>
/// Controller ?????? ??????????
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
	private readonly IUserProvider _userProvider;
	private readonly IRoleProvider _roleProvider;
	private readonly IClaimProvider _claimProvider;
	private readonly ILogger<UsersController> _logger;

	public UsersController(
		IUserProvider userProvider,
		IRoleProvider roleProvider,
		IClaimProvider claimProvider,
		ILogger<UsersController> logger)
	{
		_userProvider = userProvider ?? throw new ArgumentNullException(nameof(userProvider));
		_roleProvider = roleProvider ?? throw new ArgumentNullException(nameof(roleProvider));
		_claimProvider = claimProvider ?? throw new ArgumentNullException(nameof(claimProvider));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// ????? ?????? ????
	/// </summary>
	[HttpPost]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> CreateUser(
		[FromBody] CreateUserDto dto,
		CancellationToken cancellationToken = default)
	{
		try
		{
			if (dto.Password != dto.ConfirmPassword)
			{
				return BadRequest(new
				{
					error = "PASSWORD_MISMATCH",
					message = "Password and confirmation do not match"
				});
			}

			_logger.LogInformation("Creating new user: {Username}", dto.Username);

			var request = new CreateUserRequest
			{
				Username = dto.Username,
				Email = dto.Email,
				PhoneNumber = dto.PhoneNumber,
				Password = dto.Password,
				TwoFactorEnabled = dto.TwoFactorEnabled,
				Roles = dto.Roles,
				Metadata = dto.Metadata
			};

			var result = await _userProvider.CreateUserAsync(request, cancellationToken);

			if (!result.Success)
			{
				return BadRequest(new
				{
					error = result.Error?.Code,
					message = result.Error?.Message,
					details = result.Error?.Details
				});
			}

			return CreatedAtAction(
				nameof(GetUser),
				new { userId = result.User!.UserId },
				MapToUserResponseDto(result.User));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating user");
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ??? ?????? ???????
	/// </summary>
	[HttpGet("{userId}")]
	public async Task<IActionResult> GetUser(
		string userId,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var user = await _userProvider.GetUserByIdAsync(userId, cancellationToken);

			if (user == null)
			{
				return NotFound(new
				{
					error = "USER_NOT_FOUND",
					message = "User not found"
				});
			}

			return Ok(MapToUserResponseDto(user));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting user: {UserId}", userId);
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ??? ???????? ??????
	/// </summary>
	[HttpGet("me")]
	public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken = default)
	{
		try
		{
			var userId = User.FindFirst("sub")?.Value
				?? User.FindFirst("userId")?.Value
				?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new
				{
					error = "UNAUTHORIZED",
					message = "User ID not found in token"
				});
			}

			return await GetUser(userId, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting current user");
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ????? ??????
	/// </summary>
	[HttpPut("{userId}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> UpdateUser(
		string userId,
		[FromBody] UpdateUserDto dto,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Updating user: {UserId}", userId);

			var request = new UpdateUserRequest
			{
				Username = dto.Username,
				Email = dto.Email,
				PhoneNumber = dto.PhoneNumber,
				IsActive = dto.IsActive,
				TwoFactorEnabled = dto.TwoFactorEnabled,
				Metadata = dto.Metadata
			};

			var result = await _userProvider.UpdateUserAsync(userId, request, cancellationToken);

			if (!result.Success)
			{
				return BadRequest(new
				{
					error = result.Error?.Code,
					message = result.Error?.Message
				});
			}

			return await GetUser(userId, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating user: {UserId}", userId);
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ??? ??????
	/// </summary>
	[HttpDelete("{userId}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> DeleteUser(
		string userId,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Deleting user: {UserId}", userId);

			var success = await _userProvider.DeleteUserAsync(userId, cancellationToken);

			if (!success)
			{
				return NotFound(new
				{
					error = "USER_NOT_FOUND",
					message = "User not found"
				});
			}

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting user: {UserId}", userId);
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ????? ???? ??????
	/// </summary>
	[HttpPost("{userId}/change-password")]
	public async Task<IActionResult> ChangePassword(
		string userId,
		[FromBody] ChangePasswordDto dto,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var currentUserId = User.FindFirst("sub")?.Value
				?? User.FindFirst("userId")?.Value
				?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			var isAdmin = User.IsInRole("Admin");

			if (currentUserId != userId && !isAdmin)
			{
				return Forbid();
			}

			if (dto.NewPassword != dto.ConfirmNewPassword)
			{
				return BadRequest(new
				{
					error = "PASSWORD_MISMATCH",
					message = "New password and confirmation do not match"
				});
			}

			_logger.LogInformation("Changing password for user: {UserId}", userId);

			var result = await _userProvider.ChangePasswordAsync(
				userId,
				dto.CurrentPassword,
				dto.NewPassword,
				cancellationToken);

			if (!result.Success)
			{
				return BadRequest(new
				{
					error = result.Error?.Code,
					message = result.Error?.Message
				});
			}

			return Ok(new
			{
				success = true,
				message = "Password changed successfully"
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error changing password for user: {UserId}", userId);
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ??? ????? ????? ???? ??????
	/// </summary>
	[HttpPost("forgot-password")]
	[AllowAnonymous]
	public async Task<IActionResult> ForgotPassword(
		[FromBody] ForgotPasswordDto dto,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Password reset requested for: {Identifier}", dto.Identifier);

			var user = await _userProvider.GetUserByEmailAsync(dto.Identifier, cancellationToken)
				?? await _userProvider.GetUserByUsernameAsync(dto.Identifier, cancellationToken);

			if (user == null)
			{
				return Ok(new
				{
					success = true,
					message = "If the user exists, a password reset email will be sent"
				});
			}

			var token = await _userProvider.GeneratePasswordResetTokenAsync(
				user.UserId,
				cancellationToken);

			// TODO: ????? ?????? ??????????
			// await _emailService.SendPasswordResetEmail(user.Email, token);

			return Ok(new
			{
				success = true,
				message = "Password reset email sent"
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in forgot password");
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ????? ????? ???? ??????
	/// </summary>
	[HttpPost("reset-password")]
	[AllowAnonymous]
	public async Task<IActionResult> ResetPassword(
		[FromBody] ResetPasswordDto dto,
		[FromQuery] string userId,
		CancellationToken cancellationToken = default)
	{
		try
		{
			if (dto.NewPassword != dto.ConfirmNewPassword)
			{
				return BadRequest(new
				{
					error = "PASSWORD_MISMATCH",
					message = "Password and confirmation do not match"
				});
			}

			_logger.LogInformation("Resetting password for user: {UserId}", userId);

			var result = await _userProvider.ResetPasswordAsync(
				userId,
				dto.Token,
				dto.NewPassword,
				cancellationToken);

			if (!result.Success)
			{
				return BadRequest(new
				{
					error = result.Error?.Code,
					message = result.Error?.Message
				});
			}

			return Ok(new
			{
				success = true,
				message = "Password reset successfully"
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error resetting password");
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ????? ?????? ??????????
	/// </summary>
	[HttpPost("confirm-email")]
	[AllowAnonymous]
	public async Task<IActionResult> ConfirmEmail(
		[FromBody] ConfirmEmailDto dto,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Confirming email for user: {UserId}", dto.UserId);

			var result = await _userProvider.ConfirmEmailAsync(
				dto.UserId,
				dto.Token,
				cancellationToken);

			if (!result.Success)
			{
				return BadRequest(new
				{
					error = result.Error?.Code,
					message = result.Error?.Message
				});
			}

			return Ok(new
			{
				success = true,
				message = "Email confirmed successfully"
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error confirming email");
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ??? ???? ??????
	/// </summary>
	[HttpPost("{userId}/lock")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> LockUser(
		string userId,
		[FromBody] DateTimeOffset? lockoutEnd,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Locking user: {UserId}", userId);

			var success = await _userProvider.LockUserAsync(userId, lockoutEnd, cancellationToken);

			if (!success)
			{
				return NotFound(new
				{
					error = "USER_NOT_FOUND",
					message = "User not found"
				});
			}

			return Ok(new
			{
				success = true,
				message = "User locked successfully"
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error locking user: {UserId}", userId);
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ????? ??? ???? ??????
	/// </summary>
	[HttpPost("{userId}/unlock")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> UnlockUser(
		string userId,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Unlocking user: {UserId}", userId);

			var success = await _userProvider.UnlockUserAsync(userId, cancellationToken);

			if (!success)
			{
				return NotFound(new
				{
					error = "USER_NOT_FOUND",
					message = "User not found"
				});
			}

			return Ok(new
			{
				success = true,
				message = "User unlocked successfully"
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error unlocking user: {UserId}", userId);
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ????? Claim ????????
	/// </summary>
	[HttpPost("{userId}/claims")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> AddClaim(
		string userId,
		[FromBody] AddClaimDto dto,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation(
				"Adding claim {Type}={Value} to user: {UserId}",
				dto.ClaimType,
				dto.ClaimValue,
				userId);

			var success = await _claimProvider.AddUserClaimAsync(
				userId,
				dto.ClaimType,
				dto.ClaimValue,
				cancellationToken);

			if (!success)
			{
				return BadRequest(new
				{
					error = "CLAIM_ADD_FAILED",
					message = "Failed to add claim"
				});
			}

			return Ok(new
			{
				success = true,
				message = "Claim added successfully"
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding claim to user: {UserId}", userId);
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ????? Claim ?? ????????
	/// </summary>
	[HttpDelete("{userId}/claims")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> RemoveClaim(
		string userId,
		[FromBody] RemoveClaimDto dto,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation(
				"Removing claim {Type}={Value} from user: {UserId}",
				dto.ClaimType,
				dto.ClaimValue,
				userId);

			var success = await _claimProvider.RemoveUserClaimAsync(
				userId,
				dto.ClaimType,
				dto.ClaimValue,
				cancellationToken);

			if (!success)
			{
				return BadRequest(new
				{
					error = "CLAIM_REMOVE_FAILED",
					message = "Failed to remove claim"
				});
			}

			return Ok(new
			{
				success = true,
				message = "Claim removed successfully"
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error removing claim from user: {UserId}", userId);
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	/// <summary>
	/// ??? ???? Claims ????????
	/// </summary>
	[HttpGet("{userId}/claims")]
	public async Task<IActionResult> GetUserClaims(
		string userId,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var claims = await _claimProvider.GetAllUserClaimsAsync(userId, cancellationToken);

			return Ok(new
			{
				userId,
				claims
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting claims for user: {UserId}", userId);
			return StatusCode(500, new
			{
				error = "INTERNAL_ERROR",
				message = "An unexpected error occurred"
			});
		}
	}

	// Helper method ?????? UserInfo ??? UserResponseDto
	private UserResponseDto MapToUserResponseDto(UserInfo user)
	{
		return new UserResponseDto
		{
			UserId = user.UserId,
			Username = user.Username,
			Email = user.Email,
			PhoneNumber = user.PhoneNumber,
			EmailVerified = user.EmailVerified,
			PhoneNumberVerified = user.PhoneNumberVerified,
			TwoFactorEnabled = user.TwoFactorEnabled,
			IsActive = user.IsActive,
			IsLocked = user.IsLocked,
			LockoutEnd = user.LockoutEnd,
			LastLoginAt = user.LastLoginAt,
			Roles = user.Roles,
			Metadata = user.Metadata
		};
	}
}


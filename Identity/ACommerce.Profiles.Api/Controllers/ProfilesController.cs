using ACommerce.Profiles.DTOs;
using ACommerce.Profiles.Entities;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.CQRS.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ACommerce.Profiles.Api.Controllers;

/// <summary>
/// متحكم البروفايلات
/// </summary>
public class ProfilesController(
        IMediator mediator,
        ILogger<ProfilesController> logger) : BaseCrudController<Profile, CreateProfileDto, UpdateProfileDto, ProfileResponseDto, UpdateProfileDto>(mediator, logger)
{
    /// <summary>
    /// الحصول على بروفايل المستخدم الحالي
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ProfileResponseDto>> GetCurrentProfile()
    {
        try
        {
            // الحصول على UserId من التوكن
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token claims");
                return Unauthorized(new { message = "User ID not found in token" });
            }

            _logger.LogInformation("Getting profile for user {UserId}", userId);

            // البحث عن البروفايل بواسطة UserId
            var searchRequest = new SharedKernel.Abstractions.Queries.SmartSearchRequest
            {
                PageSize = 1,
                PageNumber = 1,
                Filters =
                [
                    new() { PropertyName = "UserId", Value = userId, Operator = SharedKernel.Abstractions.Queries.FilterOperator.Equals }
                ]
            };

            var query = new SharedKernel.CQRS.Queries.SmartSearchQuery<Profile, ProfileResponseDto> { Request = searchRequest };
            var result = await _mediator.Send(query);

            if (result.Items.Count == 0)
            {
                _logger.LogWarning("Profile not found for user {UserId}", userId);
                return NotFound(new { message = "Profile not found" });
            }

            return Ok(result.Items.First());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user profile");
            return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
        }
    }

    /// <summary>
    /// تحديث بروفايل المستخدم الحالي
    /// </summary>
    [HttpPut("me")]
    [Authorize]
    public async Task<ActionResult<ProfileResponseDto>> UpdateCurrentProfile([FromBody] UpdateProfileDto dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User ID not found in token" });

            // البحث عن البروفايل أولاً
            var searchRequest = new SharedKernel.Abstractions.Queries.SmartSearchRequest
            {
                PageSize = 1,
                PageNumber = 1,
                Filters =
                [
                    new() { PropertyName = "UserId", Value = userId, Operator = SharedKernel.Abstractions.Queries.FilterOperator.Equals }
                ]
            };

            var query = new SharedKernel.CQRS.Queries.SmartSearchQuery<Profile, ProfileResponseDto> { Request = searchRequest };
            var result = await _mediator.Send(query);

            if (result.Items.Count == 0)
                return NotFound(new { message = "Profile not found" });

            var profileId = result.Items.First().Id;

            // تحديث البروفايل
            UpdateCommand<Profile, UpdateProfileDto> updateCommand = new()
            {
                Id = profileId,
                Data = dto
            };

            var updated = await _mediator.Send(updateCommand);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating current user profile");
            return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
        }
    }

    /// <summary>
    /// الحصول على بروفايل بواسطة UserId
    /// </summary>
    [HttpGet("by-user/{userId}")]
	public async Task<ActionResult<ProfileResponseDto>> GetByUserId(string userId)
	{
		try
		{
			var searchRequest = new SharedKernel.Abstractions.Queries.SmartSearchRequest
			{
				PageSize = 1,
				PageNumber = 1,
				Filters =
                [
                    new() { PropertyName = "UserId", Value = userId, Operator = SharedKernel.Abstractions.Queries.FilterOperator.Equals }
				]
			};

			var query = new SharedKernel.CQRS.Queries.SmartSearchQuery<Profile, ProfileResponseDto> { Request = searchRequest };
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
				return NotFound(new { message = "Profile not found" });

			return Ok(result.Items.First());
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting profile by UserId {UserId}", userId);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}

	/// <summary>
	/// توثيق البروفايل (للبائعين)
	/// </summary>
	[HttpPost("{id}/verify")]
	public async Task<IActionResult> VerifyProfile(Guid id)
	{
		try
		{
			var updateDto = new UpdateProfileDto();
			// سيتم تنفيذ منطق التوثيق لاحقاً
			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error verifying profile {ProfileId}", id);
			return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
		}
	}
}

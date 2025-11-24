using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.Profiles.Entities;
using ACommerce.Profiles.DTOs;
using ACommerce.SharedKernel.AspNetCore.Controllers;

namespace ACommerce.Profiles.Api.Controllers;

/// <summary>
/// متحكم البروفايلات
/// </summary>
public class ProfilesController(
        IMediator mediator,
        ILogger<ProfilesController> logger) : BaseCrudController<Profile, CreateProfileDto, UpdateProfileDto, ProfileResponseDto, UpdateProfileDto>(mediator, logger)
{

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

using ACommerce.Notifications.Recipients.DTOs.RecipientGroup;
using ACommerce.Notifications.Recipients.DTOs.UserRecipient;
using ACommerce.Notifications.Recipients.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.Notifications.Recipients.Api.Controllers;

/// <summary>
/// ????? ??????? ????????? (Recipient Groups)
/// </summary>
[ApiController]
[Route("api/notifications/recipient-groups")]
[Produces("application/json")]
public class RecipientGroupsController : BaseCrudController<

	RecipientGroup,
	CreateRecipientGroupDto,
	UpdateRecipientGroupDto,
	RecipientGroupResponseDto,
	PartialUpdateRecipientGroupDto>
{
	public RecipientGroupsController(
		IMediator mediator,
		ILogger<RecipientGroupsController> logger)
		: base(mediator, logger)
	{
	}

	// ====================================================================================
	// ??? Endpoints ???????? ?????? ?? BaseCrudController
	// ====================================================================================

	/// <summary>
	/// ?????? ??? ????????? ?????? ???
	/// GET /api/notifications/recipient-groups/active
	/// </summary>
	[HttpGet("active")]
	[ProducesResponseType(typeof(PagedResult<RecipientGroupResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<RecipientGroupResponseDto>>> GetActive(
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 20)
	{
		try
		{
			_logger.LogDebug("Getting active recipient groups");

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(RecipientGroup.IsActive),
						Value = true,
						Operator = FilterOperator.Equals
					}
				},
				PageNumber = pageNumber,
				PageSize = pageSize,
				OrderBy = nameof(RecipientGroup.Name),
				Ascending = true
			};

			var query = new SmartSearchQuery<RecipientGroup, RecipientGroupResponseDto> { Request = searchRequest };
			var result = await _mediator.Send(query);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting active recipient groups");
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}

	/// <summary>
	/// ????? ?? ?????? ??????
	/// GET /api/notifications/recipient-groups/by-name/{name}
	/// </summary>
	[HttpGet("by-name/{name}")]
	[ProducesResponseType(typeof(RecipientGroupResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<RecipientGroupResponseDto>> GetByName(string name)
	{
		try
		{
			_logger.LogDebug("Getting recipient group by name {Name}", name);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(RecipientGroup.Name),
						Value = name,
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 1,
				PageNumber = 1
			};

			var query = new SmartSearchQuery<RecipientGroup, RecipientGroupResponseDto> { Request = searchRequest };
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				_logger.LogWarning("Recipient group with name {Name} not found", name);
				return NotFound(new { message = "Recipient group not found" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting recipient group by name {Name}", name);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}

	/// <summary>
	/// ?????? ??? ????? ??????
	/// GET /api/notifications/recipient-groups/{id}/members
	/// </summary>
	[HttpGet("{id}/members")]
	[ProducesResponseType(typeof(PagedResult<UserRecipientResponseDto>), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<UserRecipientResponseDto>>> GetMembers(
		Guid id,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 20)
	{
		try
		{
			_logger.LogDebug("Getting members of group {GroupId}", id);

			// ?????? ?? ???? ????????
			var getQuery = new GetByIdQuery<RecipientGroup, RecipientGroupResponseDto> { Id = id };
			var group = await _mediator.Send(getQuery);

			if (group == null)
			{
				_logger.LogWarning("Group {GroupId} not found", id);
				return NotFound(new { message = "Group not found" });
			}

			// TODO: ??? ????? ????????
			// ??? ????? ??? Query ???? ?? ????? SmartSearch

			return Ok(new PagedResult<UserRecipientResponseDto>
			{
				Items = new List<UserRecipientResponseDto>(),
				TotalCount = 0,
				PageNumber = pageNumber,
				PageSize = pageSize
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting members of group {GroupId}", id);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}

	/// <summary>
	/// ????? ??? ??? ??????
	/// POST /api/notifications/recipient-groups/{groupId}/members/{recipientId}
	/// </summary>
	[HttpPost("{groupId}/members/{recipientId}")]
	[ProducesResponseType(204)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> AddMember(Guid groupId, Guid recipientId)
	{
		try
		{
			_logger.LogDebug("Adding member {RecipientId} to group {GroupId}", recipientId, groupId);

			// TODO: ????? ???? ????? ?????

			_logger.LogInformation("Added member {RecipientId} to group {GroupId}", recipientId, groupId);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding member {RecipientId} to group {GroupId}", recipientId, groupId);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}

	/// <summary>
	/// ????? ??? ?? ??????
	/// DELETE /api/notifications/recipient-groups/{groupId}/members/{recipientId}
	/// </summary>
	[HttpDelete("{groupId}/members/{recipientId}")]
	[ProducesResponseType(204)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> RemoveMember(Guid groupId, Guid recipientId)
	{
		try
		{
			_logger.LogDebug("Removing member {RecipientId} from group {GroupId}", recipientId, groupId);

			// TODO: ????? ???? ????? ?????

			_logger.LogInformation("Removed member {RecipientId} from group {GroupId}", recipientId, groupId);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error removing member {RecipientId} from group {GroupId}", recipientId, groupId);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}
}


using ACommerce.Notifications.Recipients.DTOs.ContactPoint;
using ACommerce.Notifications.Recipients.DTOs.RecipientGroup;
using ACommerce.Notifications.Recipients.DTOs.UserRecipient;
using ACommerce.Notifications.Recipients.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.CQRS.Commands;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.Notifications.Recipients.Api.Controllers;

/// <summary>
/// ????? ????????? (Recipients)
/// </summary>
[ApiController]
[Route("api/notifications/recipients")]
[Produces("application/json")]
public class RecipientsController : BaseCrudController<
	UserRecipient,
	CreateUserRecipientDto,
	UpdateUserRecipientDto,
	UserRecipientResponseDto,
	PartialUpdateUserRecipientDto>
{
	public RecipientsController(
		IMediator mediator,
		ILogger<RecipientsController> logger)
		: base(mediator, logger)
	{
	}

	// ====================================================================================
	// ??? Endpoints ???????? ?????? ?? BaseCrudController:
	// - GET    /api/notifications/recipients/{id}
	// - POST   /api/notifications/recipients/search
	// - GET    /api/notifications/recipients/count
	// - POST   /api/notifications/recipients
	// - PUT    /api/notifications/recipients/{id}
	// - PATCH  /api/notifications/recipients/{id}
	// - DELETE /api/notifications/recipients/{id}
	// - POST   /api/notifications/recipients/{id}/restore
	// ====================================================================================

	// ====================================================================================
	// Custom Endpoints
	// ====================================================================================

	/// <summary>
	/// ????? ?? ????? ?????? UserId
	/// GET /api/notifications/recipients/by-user/{userId}
	/// </summary>
	[HttpGet("by-user/{userId}")]
	[ProducesResponseType(typeof(UserRecipientResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<UserRecipientResponseDto>> GetByUserId(string userId)
	{
		try
		{
			_logger.LogDebug("Getting recipient by userId {UserId}", userId);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(UserRecipient.UserId),
						Value = userId,
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 1,
				PageNumber = 1,
				IncludeProperties = new List<string> { nameof(UserRecipient.ContactPoints), nameof(UserRecipient.Groups) }
			};

			var query = new SmartSearchQuery<UserRecipient, UserRecipientResponseDto> { Request = searchRequest };
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				_logger.LogWarning("Recipient with userId {UserId} not found", userId);
				return NotFound(new { message = "Recipient not found" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting recipient by userId {UserId}", userId);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}

	/// <summary>
	/// ?????? ??? ????????? ??????? ???
	/// GET /api/notifications/recipients/active
	/// </summary>
	[HttpGet("active")]
	[ProducesResponseType(typeof(PagedResult<UserRecipientResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<UserRecipientResponseDto>>> GetActiveRecipients(
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 20)
	{
		try
		{
			_logger.LogDebug("Getting active recipients");

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(UserRecipient.IsActive),
						Value = true,
						Operator = FilterOperator.Equals
					}
				},
				PageNumber = pageNumber,
				PageSize = pageSize,
				OrderBy = nameof(UserRecipient.CreatedAt),
				Ascending = false
			};

			var query = new SmartSearchQuery<UserRecipient, UserRecipientResponseDto> { Request = searchRequest };
			var result = await _mediator.Send(query);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting active recipients");
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}

	/// <summary>
	/// ????? ?? ??????? ???? ?????
	/// GET /api/notifications/recipients/by-language/{language}
	/// </summary>
	[HttpGet("by-language/{language}")]
	[ProducesResponseType(typeof(PagedResult<UserRecipientResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<UserRecipientResponseDto>>> GetByLanguage(
		string language,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 20)
	{
		try
		{
			_logger.LogDebug("Getting recipients by language {Language}", language);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(UserRecipient.PreferredLanguage),
						Value = language.ToLower(),
						Operator = FilterOperator.Equals
					}
				},
				PageNumber = pageNumber,
				PageSize = pageSize
			};

			var query = new SmartSearchQuery<UserRecipient, UserRecipientResponseDto> { Request = searchRequest };
			var result = await _mediator.Send(query);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting recipients by language {Language}", language);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}

	/// <summary>
	/// ????? ???? ????? ??????
	/// POST /api/notifications/recipients/{id}/contact-points
	/// </summary>
	[HttpPost("{id}/contact-points")]
	[ProducesResponseType(typeof(ContactPointResponseDto), 201)]
	[ProducesResponseType(400)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<ContactPointResponseDto>> AddContactPoint(
		Guid id,
		[FromBody] CreateContactPointDto dto)
	{
		try
		{
			_logger.LogDebug("Adding contact point to recipient {RecipientId}", id);

			// ?????? ?? ???? ???????
			var getQuery = new GetByIdQuery<UserRecipient, UserRecipientResponseDto> { Id = id };
			var recipient = await _mediator.Send(getQuery);

			if (recipient == null)
			{
				_logger.LogWarning("Recipient {RecipientId} not found", id);
				return NotFound(new { message = "Recipient not found" });
			}

			// ????? ???? ???????
			var command = new CreateCommand<ContactPoint, CreateContactPointDto> { Data = dto };
			var contactPoint = await _mediator.Send(command);

			_logger.LogInformation(
				"Added contact point {ContactPointId} to recipient {RecipientId}",
				contactPoint.Id,
				id);

			// ????? ??? DTO
			var response = new ContactPointResponseDto
			{
				Id = contactPoint.Id,
				UserId = contactPoint.UserId,
				Type = contactPoint.Type,
				Value = contactPoint.Value,
				IsVerified = contactPoint.IsVerified,
				IsPrimary = contactPoint.IsPrimary,
				Metadata = contactPoint.Metadata,
				CreatedAt = contactPoint.CreatedAt,
				UpdatedAt = contactPoint.UpdatedAt
			};

			return CreatedAtAction(
				actionName: "GetContactPoint",
				controllerName: "ContactPoints",
				routeValues: new { id = contactPoint.Id },
				value: response);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding contact point to recipient {RecipientId}", id);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}

	/// <summary>
	/// ????? ????? ??? ??????
	/// POST /api/notifications/recipients/{recipientId}/groups/{groupId}
	/// </summary>
	[HttpPost("{recipientId}/groups/{groupId}")]
	[ProducesResponseType(204)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> AddToGroup(Guid recipientId, Guid groupId)
	{
		try
		{
			_logger.LogDebug(
				"Adding recipient {RecipientId} to group {GroupId}",
				recipientId,
				groupId);

			// ?????? ?? ???? ??????? ?????????
			var recipientQuery = new GetByIdQuery<UserRecipient, UserRecipientResponseDto> { Id = recipientId };
			var recipient = await _mediator.Send(recipientQuery);

			if (recipient == null)
			{
				_logger.LogWarning("Recipient {RecipientId} not found", recipientId);
				return NotFound(new { message = "Recipient not found" });
			}

			var groupQuery = new GetByIdQuery<RecipientGroup, RecipientGroupResponseDto> { Id = groupId };
			var group = await _mediator.Send(groupQuery);

			if (group == null)
			{
				_logger.LogWarning("Group {GroupId} not found", groupId);
				return NotFound(new { message = "Group not found" });
			}

			// TODO: ????? ???? ??????? ????????
			// ??? ????? ??? Custom Command ?? Service Layer

			_logger.LogInformation(
				"Added recipient {RecipientId} to group {GroupId}",
				recipientId,
				groupId);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Error adding recipient {RecipientId} to group {GroupId}",
				recipientId,
				groupId);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}

	/// <summary>
	/// ????? ????? ?? ??????
	/// DELETE /api/notifications/recipients/{recipientId}/groups/{groupId}
	/// </summary>
	[HttpDelete("{recipientId}/groups/{groupId}")]
	[ProducesResponseType(204)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> RemoveFromGroup(Guid recipientId, Guid groupId)
	{
		try
		{
			_logger.LogDebug(
				"Removing recipient {RecipientId} from group {GroupId}",
				recipientId,
				groupId);

			// TODO: ????? ???? ??????? ?? ????????

			_logger.LogInformation(
				"Removed recipient {RecipientId} from group {GroupId}",
				recipientId,
				groupId);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Error removing recipient {RecipientId} from group {GroupId}",
				recipientId,
				groupId);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}
}


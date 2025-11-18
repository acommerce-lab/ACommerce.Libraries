using ACommerce.Notifications.Recipients.DTOs.ContactPoint;
using ACommerce.Notifications.Recipients.Entities;
using ACommerce.Notifications.Recipients.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.CQRS.Commands;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.Notifications.Recipients.Api.Controllers;

/// <summary>
/// ????? ???? ??????? (Contact Points)
/// </summary>
[ApiController]
[Route("api/notifications/contact-points")]
[Produces("application/json")]
public class ContactPointsController : BaseCrudController<
	ContactPoint,
	CreateContactPointDto,
	UpdateContactPointDto,
	ContactPointResponseDto,
	PartialUpdateContactPointDto>
{
	public ContactPointsController(
		IMediator mediator,
		ILogger<ContactPointsController> logger)
		: base(mediator, logger)
	{
	}

	// ====================================================================================
	// ??? Endpoints ???????? ?????? ?? BaseCrudController
	// ====================================================================================

	/// <summary>
	/// ?????? ??? ???? ????? ?????? ????
	/// GET /api/notifications/contact-points/user/{userId}
	/// </summary>
	[HttpGet("user/{userId}")]
	[ProducesResponseType(typeof(PagedResult<ContactPointResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<ContactPointResponseDto>>> GetByUserId(
		string userId,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 20)
	{
		try
		{
			_logger.LogDebug("Getting contact points for user {UserId}", userId);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(ContactPoint.UserId),
						Value = userId,
						Operator = FilterOperator.Equals
					}
				},
				PageNumber = pageNumber,
				PageSize = pageSize,
				OrderBy = nameof(ContactPoint.CreatedAt),
				Ascending = false
			};

			var query = new SmartSearchQuery<ContactPoint, ContactPointResponseDto> { Request = searchRequest };
			var result = await _mediator.Send(query);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting contact points for user {UserId}", userId);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}

	/// <summary>
	/// ?????? ??? ???? ????? ?????? ??? ?????
	/// GET /api/notifications/contact-points/user/{userId}/type/{type}
	/// </summary>
	[HttpGet("user/{userId}/type/{type}")]
	[ProducesResponseType(typeof(PagedResult<ContactPointResponseDto>), 200)]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<ContactPointResponseDto>>> GetByUserIdAndType(
		string userId,
		ContactPointType type,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 20)
	{
		try
		{
			_logger.LogDebug("Getting {Type} contact points for user {UserId}", type, userId);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(ContactPoint.UserId),
						Value = userId,
						Operator = FilterOperator.Equals
					},
					new()
					{
						PropertyName = nameof(ContactPoint.Type),
						Value = type,
						Operator = FilterOperator.Equals
					}
				},
				PageNumber = pageNumber,
				PageSize = pageSize,
				OrderBy = nameof(ContactPoint.IsPrimary),
				Ascending = false
			};

			var query = new SmartSearchQuery<ContactPoint, ContactPointResponseDto> { Request = searchRequest };
			var result = await _mediator.Send(query);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting {Type} contact points for user {UserId}", type, userId);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}

	/// <summary>
	/// ?????? ??? ???? ??????? ???????? ??????? ??? ?????
	/// GET /api/notifications/contact-points/user/{userId}/type/{type}/primary
	/// </summary>
	[HttpGet("user/{userId}/type/{type}/primary")]
	[ProducesResponseType(typeof(ContactPointResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<ContactPointResponseDto>> GetPrimaryByUserIdAndType(
		string userId,
		ContactPointType type)
	{
		try
		{
			_logger.LogDebug("Getting primary {Type} contact point for user {UserId}", type, userId);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(ContactPoint.UserId),
						Value = userId,
						Operator = FilterOperator.Equals
					},
					new()
					{
						PropertyName = nameof(ContactPoint.Type),
						Value = type,
						Operator = FilterOperator.Equals
					},
					new()
					{
						PropertyName = nameof(ContactPoint.IsPrimary),
						Value = true,
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 1,
				PageNumber = 1
			};

			var query = new SmartSearchQuery<ContactPoint, ContactPointResponseDto> { Request = searchRequest };
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				_logger.LogWarning("No primary {Type} contact point found for user {UserId}", type, userId);
				return NotFound(new { message = "Primary contact point not found" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting primary {Type} contact point for user {UserId}", type, userId);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}

	/// <summary>
	/// ?????? ?? ???? ?????
	/// POST /api/notifications/contact-points/{id}/verify
	/// </summary>
	[HttpPost("{id}/verify")]
	[ProducesResponseType(204)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> Verify(Guid id)
	{
		try
		{
			_logger.LogDebug("Verifying contact point {ContactPointId}", id);

			// ?????? ??? ???? ???????
			var getQuery = new GetByIdQuery<ContactPoint, ContactPointResponseDto> { Id = id };
			var contactPoint = await _mediator.Send(getQuery);

			if (contactPoint == null)
			{
				_logger.LogWarning("Contact point {ContactPointId} not found", id);
				return NotFound(new { message = "Contact point not found" });
			}

			// ????? ???? ??????
			var updateDto = new PartialUpdateContactPointDto
			{
				IsVerified = true
			};

			var command = new PartialUpdateCommand<ContactPoint, PartialUpdateContactPointDto>
			{
				Id = id,
				Data = updateDto
			};

			await _mediator.Send(command);

			_logger.LogInformation("Verified contact point {ContactPointId}", id);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error verifying contact point {ContactPointId}", id);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}

	/// <summary>
	/// ????? ???? ????? ???????
	/// POST /api/notifications/contact-points/{id}/set-primary
	/// </summary>
	[HttpPost("{id}/set-primary")]
	[ProducesResponseType(204)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> SetAsPrimary(Guid id)
	{
		try
		{
			_logger.LogDebug("Setting contact point {ContactPointId} as primary", id);

			// ?????? ??? ???? ???????
			var getQuery = new GetByIdQuery<ContactPoint, ContactPointResponseDto> { Id = id };
			var contactPoint = await _mediator.Send(getQuery);

			if (contactPoint == null)
			{
				_logger.LogWarning("Contact point {ContactPointId} not found", id);
				return NotFound(new { message = "Contact point not found" });
			}

			// TODO: ????? Primary ?? ???? ??????? ?????? ?? ??? ?????
			// ?? ????? ??? ?????? ?? Primary

			var updateDto = new PartialUpdateContactPointDto
			{
				IsPrimary = true
			};

			var command = new PartialUpdateCommand<ContactPoint, PartialUpdateContactPointDto>
			{
				Id = id,
				Data = updateDto
			};

			await _mediator.Send(command);

			_logger.LogInformation("Set contact point {ContactPointId} as primary", id);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error setting contact point {ContactPointId} as primary", id);
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}

	/// <summary>
	/// ?????? ??? ???? ??????? ??????????? ???
	/// GET /api/notifications/contact-points/verified
	/// </summary>
	[HttpGet("verified")]
	[ProducesResponseType(typeof(PagedResult<ContactPointResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<ContactPointResponseDto>>> GetVerified(
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 20)
	{
		try
		{
			_logger.LogDebug("Getting verified contact points");

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(ContactPoint.IsVerified),
						Value = true,
						Operator = FilterOperator.Equals
					}
				},
				PageNumber = pageNumber,
				PageSize = pageSize,
				OrderBy = nameof(ContactPoint.CreatedAt),
				Ascending = false
			};

			var query = new SmartSearchQuery<ContactPoint, ContactPointResponseDto> { Request = searchRequest };
			var result = await _mediator.Send(query);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting verified contact points");
			return StatusCode(500, new { message = "An error occurred while processing your request", detail = ex.Message });
		}
	}
}


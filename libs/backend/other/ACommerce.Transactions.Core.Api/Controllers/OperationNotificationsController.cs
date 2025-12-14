using ACommerce.Transactions.Core.DTOs.OperationNotification;
using ACommerce.Transactions.Core.Entities;
using ACommerce.Transactions.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.Transactions.Core.Api.Controllers;

/// <summary>
/// ????? ??????? ???????? (Operation Notifications)
/// </summary>
[ApiController]
[Route("api/transactions/operation-notifications")]
[Produces("application/json")]
public class OperationNotificationsController : BaseCrudController<
	OperationNotification,
	CreateOperationNotificationDto,
	UpdateOperationNotificationDto,
	OperationNotificationResponseDto,
	PartialUpdateOperationNotificationDto>
{
	public OperationNotificationsController(
		IMediator mediator,
		ILogger<OperationNotificationsController> logger)
		: base(mediator, logger)
	{
	}

	/// <summary>
	/// ?????? ??? ????????? ??? ??? ???????
	/// GET /api/transactions/operation-notifications/by-type/{type}
	/// </summary>
	[HttpGet("by-type/{type}")]
	[ProducesResponseType(typeof(List<OperationNotificationResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<OperationNotificationResponseDto>>> GetByType(
		NotificationType type)
	{
		try
		{
			_logger.LogDebug("Getting notifications by type {Type}", type);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(OperationNotification.Type),
						Value = type,
						Operator = FilterOperator.Equals
					},
					new()
					{
						PropertyName = nameof(OperationNotification.IsActive),
						Value = true,
						Operator = FilterOperator.Equals
					}
				},
				OrderBy = nameof(OperationNotification.Priority),
				Ascending = false,
				PageSize = 100
			};

			var query = new SmartSearchQuery<OperationNotification, OperationNotificationResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting notifications by type {Type}", type);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ????????? ??? ???????
	/// GET /api/transactions/operation-notifications/by-operation/{operationId}
	/// </summary>
	[HttpGet("by-operation/{operationId}")]
	[ProducesResponseType(typeof(List<OperationNotificationResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<OperationNotificationResponseDto>>> GetByOperation(
		Guid operationId)
	{
		try
		{
			_logger.LogDebug("Getting notifications for operation {OperationId}", operationId);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(OperationNotification.OperationId),
						Value = operationId,
						Operator = FilterOperator.Equals
					},
					new()
					{
						PropertyName = nameof(OperationNotification.IsActive),
						Value = true,
						Operator = FilterOperator.Equals
					}
				},
				OrderBy = nameof(OperationNotification.Priority),
				Ascending = false,
				PageSize = 100
			};

			var query = new SmartSearchQuery<OperationNotification, OperationNotificationResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting notifications for operation {OperationId}", operationId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

// DTOs ????????
public class UpdateOperationNotificationDto
{
	public string? Template { get; set; }
	public string? Subject { get; set; }
	public List<string>? Recipients { get; set; }
	public List<string>? CcRecipients { get; set; }
	public bool? IsActive { get; set; }
	public int? DelayMinutes { get; set; }
	public int? Priority { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

public class PartialUpdateOperationNotificationDto
{
	public bool? IsActive { get; set; }
}


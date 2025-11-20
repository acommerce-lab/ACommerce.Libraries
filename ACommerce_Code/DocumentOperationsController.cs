using ACommerce.Transactions.Core.DTOs.DocumentOperation;
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
/// ????? ?????? ??????? (Document Operations)
/// </summary>
[ApiController]
[Route("api/transactions/document-operations")]
[Produces("application/json")]
public class DocumentOperationsController : BaseCrudController<
	DocumentOperation,
	CreateDocumentOperationDto,
	UpdateDocumentOperationDto,
	DocumentOperationResponseDto,
	PartialUpdateDocumentOperationDto>
{
	public DocumentOperationsController(
		IMediator mediator,
		ILogger<DocumentOperationsController> logger)
		: base(mediator, logger)
	{
	}

	/// <summary>
	/// ?????? ??? ???????? ??? ??? ???????
	/// GET /api/transactions/document-operations/by-document-type/{documentTypeId}
	/// </summary>
	[HttpGet("by-document-type/{documentTypeId}")]
	[ProducesResponseType(typeof(List<DocumentOperationResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<DocumentOperationResponseDto>>> GetByDocumentType(
		Guid documentTypeId)
	{
		try
		{
			_logger.LogDebug("Getting operations for document type {DocumentTypeId}", documentTypeId);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(DocumentOperation.DocumentTypeId),
						Value = documentTypeId,
						Operator = FilterOperator.Equals
					},
					new()
					{
						PropertyName = nameof(DocumentOperation.IsActive),
						Value = true,
						Operator = FilterOperator.Equals
					}
				},
				OrderBy = nameof(DocumentOperation.SortOrder),
				Ascending = true,
				PageSize = 100
			};

			var query = new SmartSearchQuery<DocumentOperation, DocumentOperationResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting operations for document type {DocumentTypeId}", documentTypeId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ???????? ??? ??? ???????
	/// GET /api/transactions/document-operations/by-operation-type/{operationType}
	/// </summary>
	[HttpGet("by-operation-type/{operationType}")]
	[ProducesResponseType(typeof(List<DocumentOperationResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<DocumentOperationResponseDto>>> GetByOperationType(
		OperationType operationType)
	{
		try
		{
			_logger.LogDebug("Getting operations by type {OperationType}", operationType);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(DocumentOperation.Operation),
						Value = operationType,
						Operator = FilterOperator.Equals
					},
					new()
					{
						PropertyName = nameof(DocumentOperation.IsActive),
						Value = true,
						Operator = FilterOperator.Equals
					}
				},
				OrderBy = nameof(DocumentOperation.SortOrder),
				Ascending = true,
				PageSize = 100
			};

			var query = new SmartSearchQuery<DocumentOperation, DocumentOperationResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting operations by type {OperationType}", operationType);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ???????? ???? ????? ??????
	/// GET /api/transactions/document-operations/requiring-approval
	/// </summary>
	[HttpGet("requiring-approval")]
	[ProducesResponseType(typeof(List<DocumentOperationResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<DocumentOperationResponseDto>>> GetRequiringApproval()
	{
		try
		{
			_logger.LogDebug("Getting operations requiring approval");

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(DocumentOperation.RequiresApproval),
						Value = true,
						Operator = FilterOperator.Equals
					},
					new()
					{
						PropertyName = nameof(DocumentOperation.IsActive),
						Value = true,
						Operator = FilterOperator.Equals
					}
				},
				OrderBy = nameof(DocumentOperation.SortOrder),
				Ascending = true,
				PageSize = 100
			};

			var query = new SmartSearchQuery<DocumentOperation, DocumentOperationResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting operations requiring approval");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ????????? ???????? ??????
	/// GET /api/transactions/document-operations/{id}/notifications
	/// </summary>
	[HttpGet("{id}/notifications")]
	[ProducesResponseType(typeof(List<OperationNotificationResponseDto>), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<OperationNotificationResponseDto>>> GetNotifications(Guid id)
	{
		try
		{
			_logger.LogDebug("Getting notifications for operation {OperationId}", id);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(OperationNotification.OperationId),
						Value = id,
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
			_logger.LogError(ex, "Error getting notifications for operation {OperationId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

// DTOs ????????
public class UpdateDocumentOperationDto
{
	public string? CustomName { get; set; }
	public string? Description { get; set; }
	public bool? RequiresApproval { get; set; }
	public List<string>? AllowedRoles { get; set; }
	public List<string>? ApprovalRoles { get; set; }
	public string? Conditions { get; set; }
	public bool? IsActive { get; set; }
	public int? SortOrder { get; set; }
	public string? Icon { get; set; }
	public string? ColorHex { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

public class PartialUpdateDocumentOperationDto
{
	public string? CustomName { get; set; }
	public bool? IsActive { get; set; }
}

public class OperationNotificationResponseDto
{
	public Guid Id { get; set; }
	public Guid OperationId { get; set; }
	public NotificationType Type { get; set; }
	public string Template { get; set; } = string.Empty;
	public string? Subject { get; set; }
	public List<string> Recipients { get; set; } = new();
	public List<string> CcRecipients { get; set; } = new();
	public bool IsActive { get; set; }
	public int? DelayMinutes { get; set; }
	public int Priority { get; set; }
	public DateTime CreatedAt { get; set; }
}


using ACommerce.Transactions.Core.DTOs.DocumentTypeRelation;
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
/// ????? ?????? ????? ??????? (Document Type Relations)
/// </summary>
[ApiController]
[Route("api/transactions/document-type-relations")]
[Produces("application/json")]
public class DocumentTypeRelationsController : BaseCrudController<
	DocumentTypeRelation,
	CreateDocumentTypeRelationDto,
	UpdateDocumentTypeRelationDto,
	DocumentTypeRelationResponseDto,
	PartialUpdateDocumentTypeRelationDto>
{
	public DocumentTypeRelationsController(
		IMediator mediator,
		ILogger<DocumentTypeRelationsController> logger)
		: base(mediator, logger)
	{
	}

	/// <summary>
	/// ?????? ??? ???????? ??? ??? ??????? ??????
	/// GET /api/transactions/document-type-relations/by-source/{sourceId}
	/// </summary>
	[HttpGet("by-source/{sourceId}")]
	[ProducesResponseType(typeof(List<DocumentTypeRelationResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<DocumentTypeRelationResponseDto>>> GetBySource(
		Guid sourceId)
	{
		try
		{
			_logger.LogDebug("Getting relations by source {SourceId}", sourceId);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(DocumentTypeRelation.SourceDocumentTypeId),
						Value = sourceId,
						Operator = FilterOperator.Equals
					}
				},
				OrderBy = nameof(DocumentTypeRelation.Priority),
				Ascending = true,
				PageSize = 100
			};

			var query = new SmartSearchQuery<DocumentTypeRelation, DocumentTypeRelationResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting relations by source {SourceId}", sourceId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ???????? ??? ??? ??????? ?????
	/// GET /api/transactions/document-type-relations/by-target/{targetId}
	/// </summary>
	[HttpGet("by-target/{targetId}")]
	[ProducesResponseType(typeof(List<DocumentTypeRelationResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<DocumentTypeRelationResponseDto>>> GetByTarget(
		Guid targetId)
	{
		try
		{
			_logger.LogDebug("Getting relations by target {TargetId}", targetId);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(DocumentTypeRelation.TargetDocumentTypeId),
						Value = targetId,
						Operator = FilterOperator.Equals
					}
				},
				OrderBy = nameof(DocumentTypeRelation.Priority),
				Ascending = true,
				PageSize = 100
			};

			var query = new SmartSearchQuery<DocumentTypeRelation, DocumentTypeRelationResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting relations by target {TargetId}", targetId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ???????? ??? ??? ???????
	/// GET /api/transactions/document-type-relations/by-relation-type/{relationType}
	/// </summary>
	[HttpGet("by-relation-type/{relationType}")]
	[ProducesResponseType(typeof(List<DocumentTypeRelationResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<DocumentTypeRelationResponseDto>>> GetByRelationType(
		DocumentRelationType relationType)
	{
		try
		{
			_logger.LogDebug("Getting relations by type {RelationType}", relationType);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(DocumentTypeRelation.RelationType),
						Value = relationType,
						Operator = FilterOperator.Equals
					}
				},
				OrderBy = nameof(DocumentTypeRelation.Priority),
				Ascending = true,
				PageSize = 100
			};

			var query = new SmartSearchQuery<DocumentTypeRelation, DocumentTypeRelationResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting relations by type {RelationType}", relationType);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ???????? ?????????
	/// GET /api/transactions/document-type-relations/required
	/// </summary>
	[HttpGet("required")]
	[ProducesResponseType(typeof(List<DocumentTypeRelationResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<DocumentTypeRelationResponseDto>>> GetRequired()
	{
		try
		{
			_logger.LogDebug("Getting required relations");

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(DocumentTypeRelation.IsRequired),
						Value = true,
						Operator = FilterOperator.Equals
					}
				},
				OrderBy = nameof(DocumentTypeRelation.Priority),
				Ascending = true,
				PageSize = 100
			};

			var query = new SmartSearchQuery<DocumentTypeRelation, DocumentTypeRelationResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting required relations");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

// DTOs ????????
public class UpdateDocumentTypeRelationDto
{
	public bool? IsRequired { get; set; }
	public bool? AllowMultiple { get; set; }
	public string? Conditions { get; set; }
	public int? Priority { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

public class PartialUpdateDocumentTypeRelationDto
{
	public bool? IsRequired { get; set; }
	public bool? AllowMultiple { get; set; }
}


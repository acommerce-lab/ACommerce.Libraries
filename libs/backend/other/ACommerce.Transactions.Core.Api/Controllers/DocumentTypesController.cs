using ACommerce.Transactions.Core.DTOs.DocumentType;
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
/// ????? ????? ??????? (Document Types)
/// </summary>
[ApiController]
[Route("api/transactions/document-types")]
[Produces("application/json")]
public class DocumentTypesController : BaseCrudController<
	DocumentType,
	CreateDocumentTypeDto,
	UpdateDocumentTypeDto,
	DocumentTypeResponseDto,
	PartialUpdateDocumentTypeDto>
{
	public DocumentTypesController(
		IMediator mediator,
		ILogger<DocumentTypesController> logger)
		: base(mediator, logger)
	{
	}

	/// <summary>
	/// ?????? ??? ????? ??????? ??? ???????
	/// GET /api/transactions/document-types/by-category/{category}
	/// </summary>
	[HttpGet("by-category/{category}")]
	[ProducesResponseType(typeof(List<DocumentTypeResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<DocumentTypeResponseDto>>> GetByCategory(
		DocumentCategory category)
	{
		try
		{
			_logger.LogDebug("Getting document types by category {Category}", category);

			var searchRequest = new SmartSearchRequest
			{
				Filters =
				[
					new()
					{
						PropertyName = nameof(DocumentType.Category),
						Value = category,
						Operator = FilterOperator.Equals
					},
					new()
					{
						PropertyName = nameof(DocumentType.IsActive),
						Value = true,
						Operator = FilterOperator.Equals
					}
				],
				OrderBy = nameof(DocumentType.SortOrder),
				Ascending = true,
				PageSize = 100
			};

			var query = new SmartSearchQuery<DocumentType, DocumentTypeResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting document types by category {Category}", category);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ?? ??? ????? ??????
	/// GET /api/transactions/document-types/by-code/{code}
	/// </summary>
	[HttpGet("by-code/{code}")]
	[ProducesResponseType(typeof(DocumentTypeResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<DocumentTypeResponseDto>> GetByCode(string code)
	{
		try
		{
			_logger.LogDebug("Getting document type by code {Code}", code);

			var searchRequest = new SmartSearchRequest
			{
				Filters =
				[
					new()
					{
						PropertyName = nameof(DocumentType.Code),
						Value = code,
						Operator = FilterOperator.Equals
					}
				],
				PageSize = 1,
				PageNumber = 1
			};

			var query = new SmartSearchQuery<DocumentType, DocumentTypeResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				_logger.LogWarning("Document type with code {Code} not found", code);
				return NotFound(new { message = "Document type not found" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting document type by code {Code}", code);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ????? ??????? ???? ???? ??? ???????
	/// GET /api/transactions/document-types/inventory-affecting
	/// </summary>
	[HttpGet("inventory-affecting")]
	[ProducesResponseType(typeof(List<DocumentTypeResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<DocumentTypeResponseDto>>> GetInventoryAffecting()
	{
		try
		{
			_logger.LogDebug("Getting inventory-affecting document types");

			var searchRequest = new SmartSearchRequest
			{
				Filters =
				[
					new()
					{
						PropertyName = nameof(DocumentType.AffectsInventory),
						Value = true,
						Operator = FilterOperator.Equals
					},
					new()
					{
						PropertyName = nameof(DocumentType.IsActive),
						Value = true,
						Operator = FilterOperator.Equals
					}
				],
				OrderBy = nameof(DocumentType.SortOrder),
				Ascending = true,
				PageSize = 100
			};

			var query = new SmartSearchQuery<DocumentType, DocumentTypeResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting inventory-affecting document types");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ????? ??????? ???? ???? ??? ????????
	/// GET /api/transactions/document-types/accounting-affecting
	/// </summary>
	[HttpGet("accounting-affecting")]
	[ProducesResponseType(typeof(List<DocumentTypeResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<DocumentTypeResponseDto>>> GetAccountingAffecting()
	{
		try
		{
			_logger.LogDebug("Getting accounting-affecting document types");

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(DocumentType.AffectsAccounting),
						Value = true,
						Operator = FilterOperator.Equals
					},
					new()
					{
						PropertyName = nameof(DocumentType.IsActive),
						Value = true,
						Operator = FilterOperator.Equals
					}
				},
				OrderBy = nameof(DocumentType.SortOrder),
				Ascending = true,
				PageSize = 100
			};

			var query = new SmartSearchQuery<DocumentType, DocumentTypeResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting accounting-affecting document types");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ???????? ??????? ???? ?????
	/// GET /api/transactions/document-types/{id}/operations
	/// </summary>
	[HttpGet("{id}/operations")]
	[ProducesResponseType(typeof(List<DocumentOperationResponseDto>), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<DocumentOperationResponseDto>>> GetOperations(Guid id)
	{
		try
		{
			_logger.LogDebug("Getting operations for document type {DocumentTypeId}", id);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(DocumentOperation.DocumentTypeId),
						Value = id,
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
			_logger.LogError(ex, "Error getting operations for document type {DocumentTypeId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ??????? ??????? ???? ?????
	/// GET /api/transactions/document-types/{id}/attributes
	/// </summary>
	[HttpGet("{id}/attributes")]
	[ProducesResponseType(typeof(List<DocumentTypeAttributeResponseDto>), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<DocumentTypeAttributeResponseDto>>> GetAttributes(Guid id)
	{
		try
		{
			_logger.LogDebug("Getting attributes for document type {DocumentTypeId}", id);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(DocumentTypeAttribute.DocumentTypeId),
						Value = id,
						Operator = FilterOperator.Equals
					}
				},
				OrderBy = nameof(DocumentTypeAttribute.SortOrder),
				Ascending = true,
				PageSize = 100
			};

			var query = new SmartSearchQuery<DocumentTypeAttribute, DocumentTypeAttributeResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting attributes for document type {DocumentTypeId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ????? ??????? ??????? (Sequential)
	/// GET /api/transactions/document-types/{id}/next-types
	/// </summary>
	[HttpGet("{id}/next-types")]
	[ProducesResponseType(typeof(List<DocumentTypeRelationResponseDto>), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<DocumentTypeRelationResponseDto>>> GetNextTypes(Guid id)
	{
		try
		{
			_logger.LogDebug("Getting next document types for {DocumentTypeId}", id);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(DocumentTypeRelation.SourceDocumentTypeId),
						Value = id,
						Operator = FilterOperator.Equals
					},
					new()
					{
						PropertyName = nameof(DocumentTypeRelation.RelationType),
						Value = DocumentRelationType.Sequential,
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
			_logger.LogError(ex, "Error getting next document types for {DocumentTypeId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

// DTOs ????????
public class UpdateDocumentTypeDto
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public string? Icon { get; set; }
	public string? ColorHex { get; set; }
	public string? NumberPrefix { get; set; }
	public int? NumberLength { get; set; }
	public bool? RequiresApproval { get; set; }
	public bool? AffectsInventory { get; set; }
	public bool? AffectsAccounting { get; set; }
	public bool? IsActive { get; set; }
	public int? SortOrder { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

public class PartialUpdateDocumentTypeDto
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public string? Icon { get; set; }
	public string? ColorHex { get; set; }
	public bool? IsActive { get; set; }
}

public class DocumentOperationResponseDto
{
	public Guid Id { get; set; }
	public Guid DocumentTypeId { get; set; }
	public OperationType Operation { get; set; }
	public string? CustomName { get; set; }
	public string? Description { get; set; }
	public bool RequiresApproval { get; set; }
	public List<string> AllowedRoles { get; set; } = new();
	public List<string> ApprovalRoles { get; set; } = new();
	public bool IsActive { get; set; }
	public int SortOrder { get; set; }
	public string? Icon { get; set; }
	public string? ColorHex { get; set; }
	public int NotificationsCount { get; set; }
	public DateTime CreatedAt { get; set; }
}

public class DocumentTypeAttributeResponseDto
{
	public Guid Id { get; set; }
	public Guid DocumentTypeId { get; set; }
	public Guid AttributeDefinitionId { get; set; }
	public string AttributeName { get; set; } = string.Empty;
	public string AttributeCode { get; set; } = string.Empty;
	public bool IsRequired { get; set; }
	public int SortOrder { get; set; }
	public DateTime CreatedAt { get; set; }
}

public class DocumentTypeRelationResponseDto
{
	public Guid Id { get; set; }
	public Guid SourceDocumentTypeId { get; set; }
	public string SourceDocumentTypeName { get; set; } = string.Empty;
	public Guid TargetDocumentTypeId { get; set; }
	public string TargetDocumentTypeName { get; set; } = string.Empty;
	public DocumentRelationType RelationType { get; set; }
	public bool IsRequired { get; set; }
	public bool AllowMultiple { get; set; }
	public int Priority { get; set; }
	public DateTime CreatedAt { get; set; }
}


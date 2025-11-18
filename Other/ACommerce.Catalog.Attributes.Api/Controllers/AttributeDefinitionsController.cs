using ACommerce.Catalog.Attributes.DTOs.AttributeDefinition;
using ACommerce.Catalog.Attributes.DTOs.AttributeValue;
using ACommerce.Catalog.Attributes.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.Catalog.Attributes.Api.Controllers;

/// <summary>
/// ????? ??????? ??????? (Attribute Definitions)
/// </summary>
[ApiController]
[Route("api/catalog/attribute-definitions")]
[Produces("application/json")]
public class AttributeDefinitionsController : BaseCrudController<
	AttributeDefinition,
	CreateAttributeDefinitionDto,
	UpdateAttributeDefinitionDto,
	AttributeDefinitionResponseDto,
	PartialUpdateAttributeDefinitionDto>
{
	public AttributeDefinitionsController(
		IMediator mediator,
		ILogger<AttributeDefinitionsController> logger)
		: base(mediator, logger)
	{
	}

	// ====================================================================================
	// ??? Endpoints ???????? ?????? ?? BaseCrudController:
	// - GET    /api/catalog/attribute-definitions/{id}
	// - POST   /api/catalog/attribute-definitions/search
	// - GET    /api/catalog/attribute-definitions/count
	// - POST   /api/catalog/attribute-definitions
	// - PUT    /api/catalog/attribute-definitions/{id}
	// - PATCH  /api/catalog/attribute-definitions/{id}
	// - DELETE /api/catalog/attribute-definitions/{id}
	// - POST   /api/catalog/attribute-definitions/{id}/restore
	// ====================================================================================

	// ====================================================================================
	// Custom Endpoints
	// ====================================================================================

	/// <summary>
	/// ????? ?? ????? ????? ??????
	/// GET /api/catalog/attribute-definitions/by-code/{code}
	/// </summary>
	[HttpGet("by-code/{code}")]
	[ProducesResponseType(typeof(AttributeDefinitionResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<AttributeDefinitionResponseDto>> GetByCode(string code)
	{
		try
		{
			_logger.LogDebug("Getting attribute definition by code {Code}", code);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(AttributeDefinition.Code),
						Value = code.ToLower(),
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 1,
				PageNumber = 1
			};

			var query = new SmartSearchQuery<AttributeDefinition, AttributeDefinitionResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				_logger.LogWarning("Attribute definition with code {Code} not found", code);
				return NotFound(new { message = "Attribute definition not found" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting attribute definition by code {Code}", code);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ??????? ??????? ???????
	/// GET /api/catalog/attribute-definitions/filterable
	/// </summary>
	[HttpGet("filterable")]
	[ProducesResponseType(typeof(PagedResult<AttributeDefinitionResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<AttributeDefinitionResponseDto>>> GetFilterable(
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 50)
	{
		try
		{
			_logger.LogDebug("Getting filterable attribute definitions");

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(AttributeDefinition.IsFilterable),
						Value = true,
						Operator = FilterOperator.Equals
					}
				},
				PageNumber = pageNumber,
				PageSize = pageSize,
				OrderBy = nameof(AttributeDefinition.SortOrder),
				Ascending = true
			};

			var query = new SmartSearchQuery<AttributeDefinition, AttributeDefinitionResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting filterable attribute definitions");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ??????? ?????????
	/// GET /api/catalog/attribute-definitions/required
	/// </summary>
	[HttpGet("required")]
	[ProducesResponseType(typeof(PagedResult<AttributeDefinitionResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<AttributeDefinitionResponseDto>>> GetRequired(
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 50)
	{
		try
		{
			_logger.LogDebug("Getting required attribute definitions");

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(AttributeDefinition.IsRequired),
						Value = true,
						Operator = FilterOperator.Equals
					}
				},
				PageNumber = pageNumber,
				PageSize = pageSize,
				OrderBy = nameof(AttributeDefinition.SortOrder),
				Ascending = true
			};

			var query = new SmartSearchQuery<AttributeDefinition, AttributeDefinitionResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting required attribute definitions");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ??????? ??? ?????
	/// GET /api/catalog/attribute-definitions/by-type/{type}
	/// </summary>
	[HttpGet("by-type/{type}")]
	[ProducesResponseType(typeof(PagedResult<AttributeDefinitionResponseDto>), 200)]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<AttributeDefinitionResponseDto>>> GetByType(
		Enums.AttributeType type,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 50)
	{
		try
		{
			_logger.LogDebug("Getting attribute definitions by type {Type}", type);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(AttributeDefinition.Type),
						Value = type,
						Operator = FilterOperator.Equals
					}
				},
				PageNumber = pageNumber,
				PageSize = pageSize,
				OrderBy = nameof(AttributeDefinition.SortOrder),
				Ascending = true
			};

			var query = new SmartSearchQuery<AttributeDefinition, AttributeDefinitionResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting attribute definitions by type {Type}", type);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ??? ????? ?????
	/// GET /api/catalog/attribute-definitions/{id}/values
	/// </summary>
	[HttpGet("{id}/values")]
	[ProducesResponseType(typeof(PagedResult<AttributeValueResponseDto>), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<AttributeValueResponseDto>>> GetValues(
		Guid id,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 50,
		[FromQuery] bool activeOnly = true)
	{
		try
		{
			_logger.LogDebug("Getting values for attribute definition {AttributeDefinitionId}", id);

			// ?????? ?? ???? ???????
			var getQuery = new GetByIdQuery<AttributeDefinition, AttributeDefinitionResponseDto>
			{
				Id = id
			};
			var definition = await _mediator.Send(getQuery);

			if (definition == null)
			{
				_logger.LogWarning("Attribute definition {AttributeDefinitionId} not found", id);
				return NotFound(new { message = "Attribute definition not found" });
			}

			// ????? ?? ?????
			var filters = new List<FilterItem>
			{
				new()
				{
					PropertyName = nameof(AttributeValue.AttributeDefinitionId),
					Value = id,
					Operator = FilterOperator.Equals
				}
			};

			if (activeOnly)
			{
				filters.Add(new FilterItem
				{
					PropertyName = nameof(AttributeValue.IsActive),
					Value = true,
					Operator = FilterOperator.Equals
				});
			}

			var searchRequest = new SmartSearchRequest
			{
				Filters = filters,
				PageNumber = pageNumber,
				PageSize = pageSize,
				OrderBy = nameof(AttributeValue.SortOrder),
				Ascending = true
			};

			var query = new SmartSearchQuery<AttributeValue, AttributeValueResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting values for attribute definition {AttributeDefinitionId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ???? ????? ??????
	/// POST /api/catalog/attribute-definitions/{id}/values
	/// </summary>
	[HttpPost("{id}/values")]
	[ProducesResponseType(typeof(AttributeValueResponseDto), 201)]
	[ProducesResponseType(400)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<AttributeValueResponseDto>> AddValue(
		Guid id,
		[FromBody] CreateAttributeValueDto dto)
	{
		try
		{
			_logger.LogDebug("Adding value to attribute definition {AttributeDefinitionId}", id);

			// ?????? ?? ???? ???????
			var getQuery = new GetByIdQuery<AttributeDefinition, AttributeDefinitionResponseDto>
			{
				Id = id
			};
			var definition = await _mediator.Send(getQuery);

			if (definition == null)
			{
				_logger.LogWarning("Attribute definition {AttributeDefinitionId} not found", id);
				return NotFound(new { message = "Attribute definition not found" });
			}

			// ????? AttributeDefinitionId
			dto.AttributeDefinitionId = id;

			// ????? ??????
			var command = new SharedKernel.CQRS.Commands.CreateCommand<AttributeValue, CreateAttributeValueDto>
			{
				Data = dto
			};
			var value = await _mediator.Send(command);

			_logger.LogInformation(
				"Added value {ValueId} to attribute definition {AttributeDefinitionId}",
				value.Id,
				id);

			// ????? ??? DTO
			var response = new AttributeValueResponseDto
			{
				Id = value.Id,
				AttributeDefinitionId = value.AttributeDefinitionId,
				AttributeDefinitionName = definition.Name,
				Value = value.Value,
				DisplayName = value.DisplayName,
				Code = value.Code,
				Description = value.Description,
				ColorHex = value.ColorHex,
				ImageUrl = value.ImageUrl,
				SortOrder = value.SortOrder,
				IsActive = value.IsActive,
				Metadata = value.Metadata,
				CreatedAt = value.CreatedAt,
				UpdatedAt = value.UpdatedAt
			};

			return CreatedAtAction(
				actionName: "GetValue",
				controllerName: "AttributeValues",
				routeValues: new { id = value.Id },
				value: response);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding value to attribute definition {AttributeDefinitionId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}


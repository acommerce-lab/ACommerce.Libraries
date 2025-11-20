using ACommerce.Catalog.Attributes.DTOs.AttributeValue;
using ACommerce.Catalog.Attributes.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.CQRS.Commands;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.Catalog.Attributes.Api.Controllers;

/// <summary>
/// ????? ??? ??????? (Attribute Values)
/// </summary>
[ApiController]
[Route("api/catalog/attribute-values")]
[Produces("application/json")]
public class AttributeValuesController : BaseCrudController<
	AttributeValue,
	CreateAttributeValueDto,
	UpdateAttributeValueDto,
	AttributeValueResponseDto,
	PartialUpdateAttributeValueDto>
{
	public AttributeValuesController(
		IMediator mediator,
		ILogger<AttributeValuesController> logger)
		: base(mediator, logger)
	{
	}

	// ====================================================================================
	// ??? Endpoints ???????? ?????? ?? BaseCrudController
	// ====================================================================================

	/// <summary>
	/// ????? ?? ???? ??????
	/// GET /api/catalog/attribute-values/by-code/{code}
	/// </summary>
	[HttpGet("by-code/{code}")]
	[ProducesResponseType(typeof(AttributeValueResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<AttributeValueResponseDto>> GetByCode(string code)
	{
		try
		{
			_logger.LogDebug("Getting attribute value by code {Code}", code);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(AttributeValue.Code),
						Value = code.ToLower(),
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 1,
				PageNumber = 1
			};

			var query = new SmartSearchQuery<AttributeValue, AttributeValueResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				_logger.LogWarning("Attribute value with code {Code} not found", code);
				return NotFound(new { message = "Attribute value not found" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting attribute value by code {Code}", code);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ????? ?????? ???
	/// GET /api/catalog/attribute-values/active
	/// </summary>
	[HttpGet("active")]
	[ProducesResponseType(typeof(PagedResult<AttributeValueResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<AttributeValueResponseDto>>> GetActive(
		[FromQuery] Guid? attributeDefinitionId = null,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 50)
	{
		try
		{
			_logger.LogDebug("Getting active attribute values");

			var filters = new List<FilterItem>
			{
				new()
				{
					PropertyName = nameof(AttributeValue.IsActive),
					Value = true,
					Operator = FilterOperator.Equals
				}
			};

			if (attributeDefinitionId.HasValue)
			{
				filters.Add(new FilterItem
				{
					PropertyName = nameof(AttributeValue.AttributeDefinitionId),
					Value = attributeDefinitionId.Value,
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
			_logger.LogError(ex, "Error getting active attribute values");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ?? ??? ?????
	/// GET /api/catalog/attribute-values/search-by-value
	/// </summary>
	[HttpGet("search-by-value")]
	[ProducesResponseType(typeof(PagedResult<AttributeValueResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<AttributeValueResponseDto>>> SearchByValue(
		[FromQuery] string searchTerm,
		[FromQuery] Guid? attributeDefinitionId = null,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 20)
	{
		try
		{
			_logger.LogDebug("Searching attribute values by term {SearchTerm}", searchTerm);

			var filters = new List<FilterItem>();

			if (attributeDefinitionId.HasValue)
			{
				filters.Add(new FilterItem
				{
					PropertyName = nameof(AttributeValue.AttributeDefinitionId),
					Value = attributeDefinitionId.Value,
					Operator = FilterOperator.Equals
				});
			}

			var searchRequest = new SmartSearchRequest
			{
				SearchTerm = searchTerm,
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
			_logger.LogError(ex, "Error searching attribute values by term {SearchTerm}", searchTerm);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????/????? ????
	/// PATCH /api/catalog/attribute-values/{id}/toggle-active
	/// </summary>
	[HttpPatch("{id}/toggle-active")]
	[ProducesResponseType(204)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> ToggleActive(Guid id)
	{
		try
		{
			_logger.LogDebug("Toggling active status for attribute value {ValueId}", id);

			// ?????? ??? ??????
			var getQuery = new GetByIdQuery<AttributeValue, AttributeValueResponseDto>
			{
				Id = id
			};
			var value = await _mediator.Send(getQuery);

			if (value == null)
			{
				_logger.LogWarning("Attribute value {ValueId} not found", id);
				return NotFound(new { message = "Attribute value not found" });
			}

			// ????? ??????
			var updateDto = new PartialUpdateAttributeValueDto
			{
				IsActive = !value.IsActive
			};

			var command = new PartialUpdateCommand<AttributeValue, PartialUpdateAttributeValueDto>
			{
				Id = id,
				Data = updateDto
			};

			await _mediator.Send(command);

			_logger.LogInformation(
				"Toggled active status for attribute value {ValueId} to {IsActive}",
				id,
				!value.IsActive);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error toggling active status for attribute value {ValueId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}


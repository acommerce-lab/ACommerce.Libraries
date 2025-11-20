using ACommerce.Catalog.Units.DTOs.Unit;
using ACommerce.Catalog.Units.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.CQRS.Queries;
using Unit = ACommerce.Catalog.Units.Entities.Unit;

namespace ACommerce.Catalog.Units.Api.Controllers;

/// <summary>
/// ????? ???? ?????? (Measurement Categories)
/// ???: ?????? ?????? ?????
/// </summary>
[ApiController]
[Route("api/catalog/measurement-categories")]
[Produces("application/json")]
public class MeasurementCategoriesController : BaseCrudController<
	MeasurementCategory,
	CreateMeasurementCategoryDto,
	UpdateMeasurementCategoryDto,
	MeasurementCategoryResponseDto,
	PartialUpdateMeasurementCategoryDto>
{
	public MeasurementCategoriesController(
		IMediator mediator,
		ILogger<MeasurementCategoriesController> logger)
		: base(mediator, logger)
	{
	}

	// ====================================================================================
	// Custom Endpoints
	// ====================================================================================

	/// <summary>
	/// ????? ?? ??? ??????
	/// GET /api/catalog/measurement-categories/by-code/{code}
	/// </summary>
	[HttpGet("by-code/{code}")]
	[ProducesResponseType(typeof(MeasurementCategoryResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<MeasurementCategoryResponseDto>> GetByCode(string code)
	{
		try
		{
			_logger.LogDebug("Getting measurement category by code {Code}", code);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(MeasurementCategory.CategoryCode),
						Value = code.ToLower(),
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 1,
				PageNumber = 1
			};

			var query = new SmartSearchQuery<MeasurementCategory, MeasurementCategoryResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				_logger.LogWarning("Measurement category with code {Code} not found", code);
				return NotFound(new { message = "Measurement category not found" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting measurement category by code {Code}", code);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ??????? ?? ??? ?????
	/// GET /api/catalog/measurement-categories/{id}/units
	/// </summary>
	[HttpGet("{id}/units")]
	[ProducesResponseType(typeof(PagedResult<UnitResponseDto>), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<UnitResponseDto>>> GetUnits(
		Guid id,
		[FromQuery] Guid? systemId = null,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 50)
	{
		try
		{
			_logger.LogDebug("Getting units for measurement category {CategoryId}", id);

			// ?????? ?? ???? ?????
			var getQuery = new GetByIdQuery<MeasurementCategory, MeasurementCategoryResponseDto>
			{
				Id = id
			};
			var category = await _mediator.Send(getQuery);

			if (category == null)
			{
				_logger.LogWarning("Measurement category {CategoryId} not found", id);
				return NotFound(new { message = "Measurement category not found" });
			}

			// ????? ?? ???????
			var filters = new List<FilterItem>
			{
				new()
				{
					PropertyName = nameof(Unit.MeasurementCategoryId),
					Value = id,
					Operator = FilterOperator.Equals
				}
			};

			if (systemId.HasValue)
			{
				filters.Add(new FilterItem
				{
					PropertyName = nameof(Unit.MeasurementSystemId),
					Value = systemId.Value,
					Operator = FilterOperator.Equals
				});
			}

			var searchRequest = new SmartSearchRequest
			{
				Filters = filters,
				PageNumber = pageNumber,
				PageSize = pageSize,
				OrderBy = nameof(Unit.SortOrder),
				Ascending = true
			};

			var query = new SmartSearchQuery<Unit, UnitResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting units for measurement category {CategoryId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ?????? ???????? ???? ?????
	/// GET /api/catalog/measurement-categories/{id}/base-unit
	/// </summary>
	[HttpGet("{id}/base-unit")]
	[ProducesResponseType(typeof(UnitResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<UnitResponseDto>> GetBaseUnit(Guid id)
	{
		try
		{
			_logger.LogDebug("Getting base unit for measurement category {CategoryId}", id);

			// ?????? ??? ?????
			var getQuery = new GetByIdQuery<MeasurementCategory, MeasurementCategoryResponseDto>
			{
				Id = id
			};
			var category = await _mediator.Send(getQuery);

			if (category == null)
			{
				_logger.LogWarning("Measurement category {CategoryId} not found", id);
				return NotFound(new { message = "Measurement category not found" });
			}

			if (!category.BaseUnitId.HasValue)
			{
				return NotFound(new { message = "No base unit set for this category" });
			}

			// ?????? ??? ?????? ????????
			var unitQuery = new GetByIdQuery<Unit, UnitResponseDto>
			{
				Id = category.BaseUnitId.Value
			};
			var baseUnit = await _mediator.Send(unitQuery);

			if (baseUnit == null)
			{
				return NotFound(new { message = "Base unit not found" });
			}

			return Ok(baseUnit);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting base unit for measurement category {CategoryId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

// DTOs ?????? ??? MeasurementCategory
public class CreateMeasurementCategoryDto
{
	public required string Name { get; set; }
	public required string Code { get; set; }
	public required string CategoryCode { get; set; }
	public string? Description { get; set; }
	public Guid? BaseUnitId { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

public class UpdateMeasurementCategoryDto
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public Guid? BaseUnitId { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

public class PartialUpdateMeasurementCategoryDto
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public Guid? BaseUnitId { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

public class MeasurementCategoryResponseDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Code { get; set; } = string.Empty;
	public string CategoryCode { get; set; } = string.Empty;
	public string? Description { get; set; }
	public Guid? BaseUnitId { get; set; }
	public string? BaseUnitName { get; set; }
	public int UnitsCount { get; set; }
	public Dictionary<string, string> Metadata { get; set; } = new();
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}


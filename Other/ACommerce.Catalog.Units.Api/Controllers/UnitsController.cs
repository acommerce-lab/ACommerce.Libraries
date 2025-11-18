using ACommerce.Catalog.Units.DTOs.Unit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.CQRS.Queries;
using Unit = ACommerce.Catalog.Units.Entities.Unit;

namespace ACommerce.Catalog.Units.Api.Controllers;

/// <summary>
/// ????? ??????? (Units)
/// </summary>
[ApiController]
[Route("api/catalog/units")]
[Produces("application/json")]
public class UnitsController : BaseCrudController<
	Unit,
	CreateUnitDto,
	UpdateUnitDto,
	UnitResponseDto,
	PartialUpdateUnitDto>
{
	public UnitsController(
		IMediator mediator,
		ILogger<UnitsController> logger)
		: base(mediator, logger)
	{
	}

	// ====================================================================================
	// Custom Endpoints
	// ====================================================================================

	/// <summary>
	/// ????? ?? ???? ??????
	/// GET /api/catalog/units/by-symbol/{symbol}
	/// </summary>
	[HttpGet("by-symbol/{symbol}")]
	[ProducesResponseType(typeof(UnitResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<UnitResponseDto>> GetBySymbol(string symbol)
	{
		try
		{
			_logger.LogDebug("Getting unit by symbol {Symbol}", symbol);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(Unit.Symbol),
						Value = symbol,
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 1,
				PageNumber = 1
			};

			var query = new SmartSearchQuery<Unit, UnitResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				_logger.LogWarning("Unit with symbol {Symbol} not found", symbol);
				return NotFound(new { message = "Unit not found" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting unit by symbol {Symbol}", symbol);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ?? ???? ??????
	/// GET /api/catalog/units/by-code/{code}
	/// </summary>
	[HttpGet("by-code/{code}")]
	[ProducesResponseType(typeof(UnitResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<UnitResponseDto>> GetByCode(string code)
	{
		try
		{
			_logger.LogDebug("Getting unit by code {Code}", code);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(Unit.Code),
						Value = code.ToLower(),
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 1,
				PageNumber = 1
			};

			var query = new SmartSearchQuery<Unit, UnitResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				_logger.LogWarning("Unit with code {Code} not found", code);
				return NotFound(new { message = "Unit not found" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting unit by code {Code}", code);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ??????? ???????? ???
	/// GET /api/catalog/units/standard
	/// </summary>
	[HttpGet("standard")]
	[ProducesResponseType(typeof(PagedResult<UnitResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<UnitResponseDto>>> GetStandard(
		[FromQuery] Guid? categoryId = null,
		[FromQuery] Guid? systemId = null,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 50)
	{
		try
		{
			_logger.LogDebug("Getting standard units");

			var filters = new List<FilterItem>
			{
				new()
				{
					PropertyName = nameof(Unit.IsStandard),
					Value = true,
					Operator = FilterOperator.Equals
				}
			};

			if (categoryId.HasValue)
			{
				filters.Add(new FilterItem
				{
					PropertyName = nameof(Unit.MeasurementCategoryId),
					Value = categoryId.Value,
					Operator = FilterOperator.Equals
				});
			}

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
			_logger.LogError(ex, "Error getting standard units");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ??????? ????????? ???????
	/// GET /api/catalog/units/{id}/compatible
	/// </summary>
	[HttpGet("{id}/compatible")]
	[ProducesResponseType(typeof(List<UnitResponseDto>), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<UnitResponseDto>>> GetCompatible(Guid id)
	{
		try
		{
			_logger.LogDebug("Getting compatible units for unit {UnitId}", id);

			// ?????? ??? ??????
			var getQuery = new GetByIdQuery<Unit, UnitResponseDto> { Id = id };
			var unit = await _mediator.Send(getQuery);

			if (unit == null)
			{
				_logger.LogWarning("Unit {UnitId} not found", id);
				return NotFound(new { message = "Unit not found" });
			}

			// ????? ?? ??????? ?? ??? ?????
			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(Unit.MeasurementCategoryId),
						Value = unit.MeasurementCategoryId,
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 100,
				OrderBy = nameof(Unit.SortOrder),
				Ascending = true
			};

			var query = new SmartSearchQuery<Unit, UnitResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			// ????? ?????? ????? ?? ???????
			var compatible = result.Items.Where(u => u.Id != id).ToList();

			return Ok(compatible);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting compatible units for unit {UnitId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

// DTOs ?????? ??? Unit
public class UpdateUnitDto
{
	public string? Name { get; set; }
	public string? Symbol { get; set; }
	public decimal? ConversionToBase { get; set; }
	public string? ConversionFormula { get; set; }
	public int? DecimalPlaces { get; set; }
	public string? Description { get; set; }
	public int? SortOrder { get; set; }
	public bool? IsActive { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

public class PartialUpdateUnitDto
{
	public string? Name { get; set; }
	public string? Symbol { get; set; }
	public decimal? ConversionToBase { get; set; }
	public string? ConversionFormula { get; set; }
	public int? DecimalPlaces { get; set; }
	public string? Description { get; set; }
	public int? SortOrder { get; set; }
	public bool? IsActive { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}


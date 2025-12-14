using ACommerce.Catalog.Units.DTOs.MeasurementSystem;
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
/// ????? ????? ?????? (Measurement Systems)
/// </summary>
[ApiController]
[Route("api/catalog/measurement-systems")]
[Produces("application/json")]
public class MeasurementSystemsController(
    IMediator mediator,
    ILogger<MeasurementSystemsController> logger) : BaseCrudController<
	MeasurementSystem,
	CreateMeasurementSystemDto,
	UpdateMeasurementSystemDto,
	MeasurementSystemResponseDto,
	PartialUpdateMeasurementSystemDto>(mediator, logger)
{

    // ====================================================================================
    // ??? Endpoints ???????? ?????? ?? BaseCrudController:
    // - GET    /api/catalog/measurement-systems/{id}
    // - POST   /api/catalog/measurement-systems/search
    // - GET    /api/catalog/measurement-systems/count
    // - POST   /api/catalog/measurement-systems
    // - PUT    /api/catalog/measurement-systems/{id}
    // - PATCH  /api/catalog/measurement-systems/{id}
    // - DELETE /api/catalog/measurement-systems/{id}
    // - POST   /api/catalog/measurement-systems/{id}/restore
    // ====================================================================================

    // ====================================================================================
    // Custom Endpoints
    // ====================================================================================

    /// <summary>
    /// ????? ?? ???? ???? ??????
    /// GET /api/catalog/measurement-systems/by-code/{code}
    /// </summary>
    [HttpGet("by-code/{code}")]
	[ProducesResponseType(typeof(MeasurementSystemResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<MeasurementSystemResponseDto>> GetByCode(string code)
	{
		try
		{
			_logger.LogDebug("Getting measurement system by code {Code}", code);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(MeasurementSystem.Code),
						Value = code.ToLower(),
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 1,
				PageNumber = 1
			};

			var query = new SmartSearchQuery<MeasurementSystem, MeasurementSystemResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				_logger.LogWarning("Measurement system with code {Code} not found", code);
				return NotFound(new { message = "Measurement system not found" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting measurement system by code {Code}", code);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ?????? ?????????
	/// GET /api/catalog/measurement-systems/default
	/// </summary>
	[HttpGet("default")]
	[ProducesResponseType(typeof(MeasurementSystemResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<MeasurementSystemResponseDto>> GetDefault()
	{
		try
		{
			_logger.LogDebug("Getting default measurement system");

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(MeasurementSystem.IsDefault),
						Value = true,
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 1,
				PageNumber = 1
			};

			var query = new SmartSearchQuery<MeasurementSystem, MeasurementSystemResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				_logger.LogWarning("No default measurement system found");
				return NotFound(new { message = "No default measurement system found" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting default measurement system");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ??????? ?? ???? ????
	/// GET /api/catalog/measurement-systems/{id}/units
	/// </summary>
	[HttpGet("{id}/units")]
	[ProducesResponseType(typeof(PagedResult<UnitResponseDto>), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<UnitResponseDto>>> GetUnits(
		Guid id,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 50)
	{
		try
		{
			_logger.LogDebug("Getting units for measurement system {SystemId}", id);

			// ?????? ?? ???? ??????
			var getQuery = new GetByIdQuery<MeasurementSystem, MeasurementSystemResponseDto>
			{
				Id = id
			};
			var system = await _mediator.Send(getQuery);

			if (system == null)
			{
				_logger.LogWarning("Measurement system {SystemId} not found", id);
				return NotFound(new { message = "Measurement system not found" });
			}

			// ????? ?? ???????
			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(Unit.MeasurementSystemId),
						Value = id,
						Operator = FilterOperator.Equals
					}
				},
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
			_logger.LogError(ex, "Error getting units for measurement system {SystemId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ???? ????????
	/// POST /api/catalog/measurement-systems/{id}/set-default
	/// </summary>
	[HttpPost("{id}/set-default")]
	[ProducesResponseType(204)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> SetAsDefault(Guid id)
	{
		try
		{
			_logger.LogDebug("Setting measurement system {SystemId} as default", id);

			// TODO: ????? ???? ????? Default ?? ??????? ??????
			// ?? ????? ??? ?????? ?? Default

			_logger.LogInformation("Set measurement system {SystemId} as default", id);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error setting measurement system {SystemId} as default", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

// DTOs ?????? ??? MeasurementSystem
public class UpdateMeasurementSystemDto
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public bool? IsDefault { get; set; }
	public List<string>? Countries { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

public class PartialUpdateMeasurementSystemDto
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public bool? IsDefault { get; set; }
	public List<string>? Countries { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}


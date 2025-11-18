using ACommerce.Accounting.Core.DTOs.CostCenter;
using ACommerce.Accounting.Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.Accounting.Core.Api.Controllers;

/// <summary>
/// ????? ????? ??????? (Cost Centers)
/// </summary>
[ApiController]
[Route("api/accounting/cost-centers")]
[Produces("application/json")]
public class CostCentersController : BaseCrudController<
	CostCenter,
	CreateCostCenterDto,
	UpdateCostCenterDto,
	CostCenterResponseDto,
	PartialUpdateCostCenterDto>
{
	public CostCentersController(
		IMediator mediator,
		ILogger<CostCentersController> logger)
		: base(mediator, logger)
	{
	}

	/// <summary>
	/// ????? ??????
	/// GET /api/accounting/cost-centers/by-code/{code}
	/// </summary>
	[HttpGet("by-code/{code}")]
	[ProducesResponseType(typeof(CostCenterResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<CostCenterResponseDto>> GetByCode(string code)
	{
		try
		{
			_logger.LogDebug("Getting cost center by code {Code}", code);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(CostCenter.Code),
						Value = code,
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 1
			};

			var query = new SmartSearchQuery<CostCenter, CostCenterResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				return NotFound(new { message = "Cost center not found" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting cost center by code {Code}", code);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ??????? ???????
	/// GET /api/accounting/cost-centers/root
	/// </summary>
	[HttpGet("root")]
	[ProducesResponseType(typeof(List<CostCenterResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<CostCenterResponseDto>>> GetRoot()
	{
		try
		{
			_logger.LogDebug("Getting root cost centers");

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(CostCenter.ParentCostCenterId),
						Value = null,
						Operator = FilterOperator.IsNull
					},
					new()
					{
						PropertyName = nameof(CostCenter.IsActive),
						Value = true,
						Operator = FilterOperator.Equals
					}
				},
				OrderBy = nameof(CostCenter.Code),
				Ascending = true,
				PageSize = 100
			};

			var query = new SmartSearchQuery<CostCenter, CostCenterResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting root cost centers");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ??????? ???????
	/// GET /api/accounting/cost-centers/{id}/sub-centers
	/// </summary>
	[HttpGet("{id}/sub-centers")]
	[ProducesResponseType(typeof(List<CostCenterResponseDto>), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<CostCenterResponseDto>>> GetSubCenters(Guid id)
	{
		try
		{
			_logger.LogDebug("Getting sub cost centers for {CostCenterId}", id);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(CostCenter.ParentCostCenterId),
						Value = id,
						Operator = FilterOperator.Equals
					}
				},
				OrderBy = nameof(CostCenter.Code),
				Ascending = true,
				PageSize = 100
			};

			var query = new SmartSearchQuery<CostCenter, CostCenterResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting sub cost centers for {CostCenterId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

// DTOs
public class UpdateCostCenterDto
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public bool? IsActive { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

public class PartialUpdateCostCenterDto
{
	public string? Name { get; set; }
	public bool? IsActive { get; set; }
}


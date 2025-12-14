using ACommerce.Catalog.Currencies.DTOs.Currency;
using ACommerce.Catalog.Currencies.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.Catalog.Currencies.Api.Controllers;

/// <summary>
/// ????? ??????? (Currencies)
/// </summary>
[ApiController]
[Route("api/catalog/currencies")]
[Produces("application/json")]
public class CurrenciesController : BaseCrudController<
	Currency,
	CreateCurrencyDto,
	UpdateCurrencyDto,
	CurrencyResponseDto,
	PartialUpdateCurrencyDto>
{
	public CurrenciesController(
		IMediator mediator,
		ILogger<CurrenciesController> logger)
		: base(mediator, logger)
	{
	}

	// ====================================================================================
	// ??? Endpoints ???????? ?????? ?? BaseCrudController:
	// - GET    /api/catalog/currencies/{id}
	// - POST   /api/catalog/currencies/search
	// - GET    /api/catalog/currencies/count
	// - POST   /api/catalog/currencies
	// - PUT    /api/catalog/currencies/{id}
	// - PATCH  /api/catalog/currencies/{id}
	// - DELETE /api/catalog/currencies/{id}
	// - POST   /api/catalog/currencies/{id}/restore
	// ====================================================================================

	// ====================================================================================
	// Custom Endpoints
	// ====================================================================================

	/// <summary>
	/// ????? ?? ???? ?????? (ISO 4217)
	/// GET /api/catalog/currencies/by-code/{code}
	/// </summary>
	[HttpGet("by-code/{code}")]
	[ProducesResponseType(typeof(CurrencyResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<CurrencyResponseDto>> GetByCode(string code)
	{
		try
		{
			_logger.LogDebug("Getting currency by code {Code}", code);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(Currency.Code),
						Value = code.ToUpper(),
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 1,
				PageNumber = 1
			};

			var query = new SmartSearchQuery<Currency, CurrencyResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				_logger.LogWarning("Currency with code {Code} not found", code);
				return NotFound(new { message = "Currency not found" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting currency by code {Code}", code);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ?????? ???????? (Base Currency)
	/// GET /api/catalog/currencies/base
	/// </summary>
	[HttpGet("base")]
	[ProducesResponseType(typeof(CurrencyResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<CurrencyResponseDto>> GetBaseCurrency()
	{
		try
		{
			_logger.LogDebug("Getting base currency");

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(Currency.IsBaseCurrency),
						Value = true,
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 1,
				PageNumber = 1
			};

			var query = new SmartSearchQuery<Currency, CurrencyResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				_logger.LogWarning("No base currency found");
				return NotFound(new { message = "No base currency set" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting base currency");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ??????? ?????? ???
	/// GET /api/catalog/currencies/active
	/// </summary>
	[HttpGet("active")]
	[ProducesResponseType(typeof(PagedResult<CurrencyResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<CurrencyResponseDto>>> GetActive(
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 50)
	{
		try
		{
			_logger.LogDebug("Getting active currencies");

			var searchRequest = new SmartSearchRequest
			{
				Filters =
				[
					new()
					{
						PropertyName = nameof(Currency.IsActive),
						Value = true,
						Operator = FilterOperator.Equals
					}
				],
				PageNumber = pageNumber,
				PageSize = pageSize,
				OrderBy = nameof(Currency.Code),
				Ascending = true
			};

			var query = new SmartSearchQuery<Currency, CurrencyResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting active currencies");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ?? ????? ???????
	/// GET /api/catalog/currencies/by-country/{country}
	/// </summary>
	[HttpGet("by-country/{country}")]
	[ProducesResponseType(typeof(List<CurrencyResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<CurrencyResponseDto>>> GetByCountry(string country)
	{
		try
		{
			_logger.LogDebug("Getting currencies by country {Country}", country);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(Currency.Countries),
						Value = country,
						Operator = FilterOperator.Contains
					}
				},
				PageSize = 100
			};

			var query = new SmartSearchQuery<Currency, CurrencyResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting currencies by country {Country}", country);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ???? ????? ??????
	/// POST /api/catalog/currencies/{id}/set-as-base
	/// </summary>
	[HttpPost("{id}/set-as-base")]
	[ProducesResponseType(204)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> SetAsBaseCurrency(Guid id)
	{
		try
		{
			_logger.LogDebug("Setting currency {CurrencyId} as base currency", id);

			// TODO: ????? ???? ????? Base ?? ??????? ??????
			// ?? ????? ??? ?????? ?? Base

			_logger.LogInformation("Set currency {CurrencyId} as base currency", id);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error setting currency {CurrencyId} as base", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ???? ?????
	/// POST /api/catalog/currencies/{id}/format
	/// </summary>
	[HttpPost("{id}/format")]
	[ProducesResponseType(typeof(string), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<string>> FormatAmount(
		Guid id,
		[FromBody] FormatAmountRequest request)
	{
		try
		{
			_logger.LogDebug("Formatting amount {Amount} for currency {CurrencyId}", request.Amount, id);

			var getQuery = new GetByIdQuery<Currency, CurrencyResponseDto> { Id = id };
			var currency = await _mediator.Send(getQuery);

			if (currency == null)
			{
				return NotFound(new { message = "Currency not found" });
			}

			// TODO: ??????? CurrencyConversionService.FormatAmount
			// var formatted = _conversionService.FormatAmount(request.Amount, currency);

			return Ok("Formatted amount placeholder");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error formatting amount for currency {CurrencyId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

// DTOs ??????
public class UpdateCurrencyDto
{
	public string? Name { get; set; }
	public string? Symbol { get; set; }
	public int? DecimalPlaces { get; set; }
	public bool? SymbolBeforeAmount { get; set; }
	public string? ThousandsSeparator { get; set; }
	public string? DecimalSeparator { get; set; }
	public bool? IsBaseCurrency { get; set; }
	public bool? IsActive { get; set; }
	public List<string>? Countries { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

public class PartialUpdateCurrencyDto
{
	public string? Name { get; set; }
	public string? Symbol { get; set; }
	public int? DecimalPlaces { get; set; }
	public bool? SymbolBeforeAmount { get; set; }
	public string? ThousandsSeparator { get; set; }
	public string? DecimalSeparator { get; set; }
	public bool? IsBaseCurrency { get; set; }
	public bool? IsActive { get; set; }
	public List<string>? Countries { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

public class FormatAmountRequest
{
	public decimal Amount { get; set; }
}


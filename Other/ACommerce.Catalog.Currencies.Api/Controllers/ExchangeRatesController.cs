using ACommerce.Catalog.Currencies.DTOs.ExchangeRate;
using ACommerce.Catalog.Currencies.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.CQRS.Commands;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.Catalog.Currencies.Api.Controllers;

/// <summary>
/// ????? ????? ????? (Exchange Rates)
/// </summary>
[ApiController]
[Route("api/catalog/exchange-rates")]
[Produces("application/json")]
public class ExchangeRatesController : BaseCrudController<
	ExchangeRate,
	CreateExchangeRateDto,
	UpdateExchangeRateDto,
	ExchangeRateResponseDto,
	PartialUpdateExchangeRateDto>
{
	public ExchangeRatesController(
		IMediator mediator,
		ILogger<ExchangeRatesController> logger)
		: base(mediator, logger)
	{
	}

	// ====================================================================================
	// Custom Endpoints
	// ====================================================================================

	/// <summary>
	/// ?????? ??? ??? ????? ?????? ??? ??????
	/// GET /api/catalog/exchange-rates/current
	/// </summary>
	[HttpGet("current")]
	[ProducesResponseType(typeof(ExchangeRateResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<ExchangeRateResponseDto>> GetCurrentRate(
		[FromQuery] Guid fromCurrencyId,
		[FromQuery] Guid toCurrencyId,
		[FromQuery] string? rateType = null)
	{
		try
		{
			_logger.LogDebug(
				"Getting current exchange rate from {FromCurrencyId} to {ToCurrencyId}",
				fromCurrencyId,
				toCurrencyId);

			rateType ??= "official";

			var filters = new List<FilterItem>
			{
				new()
				{
					PropertyName = nameof(ExchangeRate.FromCurrencyId),
					Value = fromCurrencyId,
					Operator = FilterOperator.Equals
				},
				new()
				{
					PropertyName = nameof(ExchangeRate.ToCurrencyId),
					Value = toCurrencyId,
					Operator = FilterOperator.Equals
				},
				new()
				{
					PropertyName = nameof(ExchangeRate.RateType),
					Value = rateType,
					Operator = FilterOperator.Equals
				},
				new()
				{
					PropertyName = nameof(ExchangeRate.EffectiveDate),
					Value = DateTime.UtcNow,
					Operator = FilterOperator.LessThanOrEqual
				}
			};

			var searchRequest = new SmartSearchRequest
			{
				Filters = filters,
				OrderBy = nameof(ExchangeRate.EffectiveDate),
				Ascending = false,
				PageSize = 1
			};

			var query = new SmartSearchQuery<ExchangeRate, ExchangeRateResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				_logger.LogWarning(
					"No current exchange rate found from {FromCurrencyId} to {ToCurrencyId}",
					fromCurrencyId,
					toCurrencyId);
				return NotFound(new { message = "Exchange rate not found" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting current exchange rate");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ????? ????? ?????
	/// GET /api/catalog/exchange-rates/history
	/// </summary>
	[HttpGet("history")]
	[ProducesResponseType(typeof(List<ExchangeRateResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<ExchangeRateResponseDto>>> GetHistory(
		[FromQuery] Guid fromCurrencyId,
		[FromQuery] Guid toCurrencyId,
		[FromQuery] DateTime? startDate = null,
		[FromQuery] DateTime? endDate = null,
		[FromQuery] string? rateType = null)
	{
		try
		{
			_logger.LogDebug(
				"Getting exchange rate history from {FromCurrencyId} to {ToCurrencyId}",
				fromCurrencyId,
				toCurrencyId);

			startDate ??= DateTime.UtcNow.AddMonths(-6);
			endDate ??= DateTime.UtcNow;
			rateType ??= "official";

			var filters = new List<FilterItem>
			{
				new()
				{
					PropertyName = nameof(ExchangeRate.FromCurrencyId),
					Value = fromCurrencyId,
					Operator = FilterOperator.Equals
				},
				new()
				{
					PropertyName = nameof(ExchangeRate.ToCurrencyId),
					Value = toCurrencyId,
					Operator = FilterOperator.Equals
				},
				new()
				{
					PropertyName = nameof(ExchangeRate.RateType),
					Value = rateType,
					Operator = FilterOperator.Equals
				},
				new()
				{
					PropertyName = nameof(ExchangeRate.EffectiveDate),
					Value = startDate,
					SecondValue = endDate,
					Operator = FilterOperator.Between
				}
			};

			var searchRequest = new SmartSearchRequest
			{
				Filters = filters,
				OrderBy = nameof(ExchangeRate.EffectiveDate),
				Ascending = true,
				PageSize = 1000
			};

			var query = new SmartSearchQuery<ExchangeRate, ExchangeRateResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting exchange rate history");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ???? ????? ????? ????? ?????
	/// GET /api/catalog/exchange-rates/from-currency/{currencyId}
	/// </summary>
	[HttpGet("from-currency/{currencyId}")]
	[ProducesResponseType(typeof(List<ExchangeRateResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<ExchangeRateResponseDto>>> GetByCurrency(
		Guid currencyId,
		[FromQuery] string? rateType = null)
	{
		try
		{
			_logger.LogDebug("Getting exchange rates from currency {CurrencyId}", currencyId);

			rateType ??= "official";

			var filters = new List<FilterItem>
			{
				new()
				{
					PropertyName = nameof(ExchangeRate.FromCurrencyId),
					Value = currencyId,
					Operator = FilterOperator.Equals
				},
				new()
				{
					PropertyName = nameof(ExchangeRate.RateType),
					Value = rateType,
					Operator = FilterOperator.Equals
				},
				new()
				{
					PropertyName = nameof(ExchangeRate.EffectiveDate),
					Value = DateTime.UtcNow,
					Operator = FilterOperator.LessThanOrEqual
				}
			};

			var searchRequest = new SmartSearchRequest
			{
				Filters = filters,
				OrderBy = nameof(ExchangeRate.EffectiveDate),
				Ascending = false,
				PageSize = 100
			};

			var query = new SmartSearchQuery<ExchangeRate, ExchangeRateResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			// ????? ??????? ??? ?????? ????? (?????? ???)
			var latestRates = result.Items
				.GroupBy(r => r.ToCurrencyId)
				.Select(g => g.First())
				.ToList();

			return Ok(latestRates);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting exchange rates from currency {CurrencyId}", currencyId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ??? ??? (????? ??? ????)
	/// POST /api/catalog/exchange-rates/update-rate
	/// </summary>
	[HttpPost("update-rate")]
	[ProducesResponseType(typeof(ExchangeRateResponseDto), 201)]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<ExchangeRateResponseDto>> UpdateRate(
		[FromBody] UpdateRateRequest request)
	{
		try
		{
			_logger.LogDebug(
				"Updating exchange rate from {FromCurrencyId} to {ToCurrencyId}",
				request.FromCurrencyId,
				request.ToCurrencyId);

			var dto = new CreateExchangeRateDto
			{
				FromCurrencyId = request.FromCurrencyId,
				ToCurrencyId = request.ToCurrencyId,
				Rate = request.Rate,
				EffectiveDate = request.EffectiveDate ?? DateTime.UtcNow,
				Source = request.Source,
				RateType = request.RateType ?? "official",
				Priority = 1
			};

			var command = new CreateCommand<ExchangeRate, CreateExchangeRateDto>
			{
				Data = dto
			};
			var exchangeRate = await _mediator.Send(command);

			_logger.LogInformation(
				"Updated exchange rate from {FromCurrencyId} to {ToCurrencyId} to {Rate}",
				request.FromCurrencyId,
				request.ToCurrencyId,
				request.Rate);

			// TODO: ????? ??? Response DTO
			var response = new ExchangeRateResponseDto
			{
				Id = exchangeRate.Id,
				FromCurrencyId = exchangeRate.FromCurrencyId,
				ToCurrencyId = exchangeRate.ToCurrencyId,
				Rate = exchangeRate.Rate,
				InverseRate = exchangeRate.InverseRate,
				EffectiveDate = exchangeRate.EffectiveDate,
				Source = exchangeRate.Source,
				RateType = exchangeRate.RateType,
				CreatedAt = exchangeRate.CreatedAt
			};

			return CreatedAtAction(
				nameof(GetById),
				new { id = exchangeRate.Id },
				response);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating exchange rate");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

// DTOs ??????
public class UpdateExchangeRateDto
{
	public decimal? Rate { get; set; }
	public DateTime? EffectiveDate { get; set; }
	public DateTime? ExpiryDate { get; set; }
	public string? Source { get; set; }
	public int? Priority { get; set; }
}

public class PartialUpdateExchangeRateDto
{
	public decimal? Rate { get; set; }
	public DateTime? EffectiveDate { get; set; }
	public DateTime? ExpiryDate { get; set; }
	public string? Source { get; set; }
	public int? Priority { get; set; }
}

public class UpdateRateRequest
{
	public Guid FromCurrencyId { get; set; }
	public Guid ToCurrencyId { get; set; }
	public decimal Rate { get; set; }
	public DateTime? EffectiveDate { get; set; }
	public string? Source { get; set; }
	public string? RateType { get; set; }
}


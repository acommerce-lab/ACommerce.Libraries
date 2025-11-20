using ACommerce.Catalog.Currencies.DTOs;
using ACommerce.Catalog.Currencies.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ACommerce.Catalog.Currencies.Api.Controllers;

/// <summary>
/// ?????? ????? ???????
/// </summary>
[ApiController]
[Route("api/catalog/currency-conversions")]
[Produces("application/json")]
public class CurrencyConversionsController : ControllerBase
{
	private readonly ICurrencyConversionService _conversionService;
	private readonly ILogger<CurrencyConversionsController> _logger;

	public CurrencyConversionsController(
		ICurrencyConversionService conversionService,
		ILogger<CurrencyConversionsController> logger)
	{
		_conversionService = conversionService ?? throw new ArgumentNullException(nameof(conversionService));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// ????? ?? ???? ??? ????
	/// POST /api/catalog/currency-conversions/convert
	/// </summary>
	[HttpPost("convert")]
	[ProducesResponseType(typeof(CurrencyConversionResponse), 200)]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<CurrencyConversionResponse>> Convert(
		[FromBody] CurrencyConversionRequest request)
	{
		try
		{
			_logger.LogDebug(
				"Converting {Amount} from {FromCurrencyId} to {ToCurrencyId}",
				request.Amount,
				request.FromCurrencyId,
				request.ToCurrencyId);

			var convertedAmount = await _conversionService.ConvertAsync(
				request.Amount,
				request.FromCurrencyId,
				request.ToCurrencyId,
				request.Date,
				request.RateType);

			var rate = await _conversionService.ConvertAsync(
				1,
				request.FromCurrencyId,
				request.ToCurrencyId,
				request.Date,
				request.RateType);

			// TODO: ?????? ??? ?????? ??????? ???????
			var response = new CurrencyConversionResponse
			{
				OriginalAmount = request.Amount,
				ConvertedAmount = convertedAmount,
				ExchangeRate = rate,
				RateDate = request.Date ?? DateTime.UtcNow
			};

			return Ok(response);
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning(ex, "Invalid currency conversion request");
			return BadRequest(new { message = ex.Message });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error converting currencies");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ??? ?????
	/// GET /api/catalog/currency-conversions/rate
	/// </summary>
	[HttpGet("rate")]
	[ProducesResponseType(typeof(decimal), 200)]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<decimal>> GetRate(
		[FromQuery] Guid fromCurrencyId,
		[FromQuery] Guid toCurrencyId,
		[FromQuery] DateTime? date = null,
		[FromQuery] string? rateType = null)
	{
		try
		{
			_logger.LogDebug(
				"Getting exchange rate from {FromCurrencyId} to {ToCurrencyId}",
				fromCurrencyId,
				toCurrencyId);

			var rate = await _conversionService.ConvertAsync(
				1,
				fromCurrencyId,
				toCurrencyId,
				date,
				rateType);

			return Ok(rate);
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning(ex, "Invalid exchange rate request");
			return BadRequest(new { message = ex.Message });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting exchange rate");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ????? (Batch Conversion)
	/// POST /api/catalog/currency-conversions/batch-convert
	/// </summary>
	[HttpPost("batch-convert")]
	[ProducesResponseType(typeof(List<CurrencyConversionResponse>), 200)]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<CurrencyConversionResponse>>> BatchConvert(
		[FromBody] BatchCurrencyConversionRequest request)
	{
		try
		{
			_logger.LogDebug("Batch converting {Count} amounts", request.Conversions.Count);

			var responses = new List<CurrencyConversionResponse>();

			foreach (var conversion in request.Conversions)
			{
				var response = await Convert(conversion);
				if (response.Result is OkObjectResult okResult &&
					okResult.Value is CurrencyConversionResponse conversionResponse)
				{
					responses.Add(conversionResponse);
				}
			}

			return Ok(responses);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in batch currency conversion");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

/// <summary>
/// ??? ????? ?????
/// </summary>
public class BatchCurrencyConversionRequest
{
	public List<CurrencyConversionRequest> Conversions { get; set; } = new();
}


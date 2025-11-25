using ACommerce.Catalog.Units.DTOs;
using ACommerce.Catalog.Units.DTOs.Unit;
using ACommerce.Catalog.Units.Entities;
using ACommerce.Catalog.Units.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.CQRS.Queries;
using Unit = ACommerce.Catalog.Units.Entities.Unit;

namespace ACommerce.Catalog.Units.Api.Controllers;

/// <summary>
/// ?????? ????? ???????
/// </summary>
[ApiController]
[Route("api/catalog/unit-conversions")]
[Produces("application/json")]
public class UnitConversionsController : ControllerBase
{
	private readonly IUnitConversionService _conversionService;
	private readonly IMediator _mediator;
	private readonly ILogger<UnitConversionsController> _logger;

	public UnitConversionsController(
		IUnitConversionService conversionService,
		IMediator mediator,
		ILogger<UnitConversionsController> logger)
	{
		_conversionService = conversionService ?? throw new ArgumentNullException(nameof(conversionService));
		_mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// ????? ???? ?? ???? ??? ????
	/// POST /api/catalog/unit-conversions/convert
	/// </summary>
	[HttpPost("convert")]
	[ProducesResponseType(typeof(ConversionResponse), 200)]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<ConversionResponse>> Convert(
		[FromBody] ConversionRequest request)
	{
		try
		{
			_logger.LogDebug(
				"Converting {Value} from unit {FromUnitId} to unit {ToUnitId}",
				request.Value,
				request.FromUnitId,
				request.ToUnitId);

			// ?????? ??? ????????
			var fromUnitQuery = new GetByIdQuery<Unit, UnitResponseDto>
			{
				Id = request.FromUnitId
			};
			var fromUnit = await _mediator.Send(fromUnitQuery);

			var toUnitQuery = new GetByIdQuery<Unit, UnitResponseDto>
			{
				Id = request.ToUnitId
			};
			var toUnit = await _mediator.Send(toUnitQuery);

			if (fromUnit == null || toUnit == null)
			{
				return BadRequest(new { message = "One or both units not found" });
			}

			// ???????
			var convertedValue = await _conversionService.ConvertAsync(
				request.Value,
				request.FromUnitId,
				request.ToUnitId);

			var conversionFactor = await _conversionService.ConvertAsync(
				1,
				request.FromUnitId,
				request.ToUnitId);

			var response = new ConversionResponse
			{
				OriginalValue = request.Value,
				FromUnit = fromUnit.Name,
				FromSymbol = fromUnit.Symbol,
				ConvertedValue = convertedValue,
				ToUnit = toUnit.Name,
				ToSymbol = toUnit.Symbol,
				ConversionFactor = conversionFactor,
				Formula = $"1 {fromUnit.Symbol} = {conversionFactor} {toUnit.Symbol}"
			};

			_logger.LogInformation(
				"Converted {Value} {FromSymbol} to {ConvertedValue} {ToSymbol}",
				request.Value,
				fromUnit.Symbol,
				convertedValue,
				toUnit.Symbol);

			return Ok(response);
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning(ex, "Invalid conversion request");
			return BadRequest(new { message = ex.Message });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error converting units");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ????? ??????? ??? ??????
	/// GET /api/catalog/unit-conversions/factor
	/// </summary>
	[HttpGet("factor")]
	[ProducesResponseType(typeof(decimal), 200)]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<decimal>> GetConversionFactor(
		[FromQuery] Guid fromUnitId,
		[FromQuery] Guid toUnitId)
	{
		try
		{
			_logger.LogDebug(
				"Getting conversion factor from {FromUnitId} to {ToUnitId}",
				fromUnitId,
				toUnitId);

			var factor = await _conversionService.ConvertAsync(1, fromUnitId, toUnitId);

			return Ok(factor);
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning(ex, "Invalid conversion factor request");
			return BadRequest(new { message = ex.Message });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting conversion factor");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ?? ??????? ??????? ??? ??????
	/// GET /api/catalog/unit-conversions/can-convert
	/// </summary>
	[HttpGet("can-convert")]
	[ProducesResponseType(typeof(bool), 200)]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<bool>> CanConvert(
		[FromQuery] Guid fromUnitId,
		[FromQuery] Guid toUnitId)
	{
		try
		{
			_logger.LogDebug(
				"Checking if can convert from {FromUnitId} to {ToUnitId}",
				fromUnitId,
				toUnitId);

			// ?????? ??? ????????
			var fromUnitQuery = new GetByIdQuery<Unit, UnitResponseDto>
			{
				Id = fromUnitId
			};
			var fromUnit = await _mediator.Send(fromUnitQuery);

			var toUnitQuery = new GetByIdQuery<Unit, UnitResponseDto>
			{
				Id = toUnitId
			};
			var toUnit = await _mediator.Send(toUnitQuery);

			if (fromUnit == null || toUnit == null)
			{
				return BadRequest(new { message = "One or both units not found" });
			}

			// ??? ??????
			var canConvert = fromUnit.UnitCategoryId == toUnit.UnitCategoryId;

			return Ok(canConvert);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error checking conversion possibility");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ????? (Batch Conversion)
	/// POST /api/catalog/unit-conversions/batch-convert
	/// </summary>
	[HttpPost("batch-convert")]
	[ProducesResponseType(typeof(List<ConversionResponse>), 200)]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<ConversionResponse>>> BatchConvert(
		[FromBody] BatchConversionRequest request)
	{
		try
		{
			_logger.LogDebug(
				"Batch converting {Count} values",
				request.Conversions.Count);

			var responses = new List<ConversionResponse>();

			foreach (var conversion in request.Conversions)
			{
				var response = await Convert(conversion);
				if (response.Result is OkObjectResult okResult && okResult.Value is ConversionResponse conversionResponse)
				{
					responses.Add(conversionResponse);
				}
			}

			return Ok(responses);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in batch conversion");
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
public class BatchConversionRequest
{
	public List<ConversionRequest> Conversions { get; set; } = new();
}


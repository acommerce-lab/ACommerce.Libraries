using ACommerce.Catalog.Currencies.Entities;
using ACommerce.Catalog.Units.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Repositories;

namespace ACommerce.Catalog.Simple.Api.Controllers;

/// <summary>
/// ????? ???? ???????? - ????? ?????? ??????? ??????????
/// </summary>
[ApiController]
[Route("api/simple-catalog/setup")]
[Produces("application/json")]
public class SimpleSetupController : ControllerBase
{
	private readonly IBaseAsyncRepository<Currency> _currencyRepository;
	private readonly IBaseAsyncRepository<UnitCategory> _categoryRepository;
	private readonly IBaseAsyncRepository<MeasurementSystem> _systemRepository;
	private readonly IBaseAsyncRepository<Unit> _unitRepository;
	private readonly ILogger<SimpleSetupController> _logger;

	public SimpleSetupController(
		IBaseAsyncRepository<Currency> currencyRepository,
		IBaseAsyncRepository<UnitCategory> categoryRepository,
		IBaseAsyncRepository<MeasurementSystem> systemRepository,
		IBaseAsyncRepository<Unit> unitRepository,
		ILogger<SimpleSetupController> logger)
	{
		_currencyRepository = currencyRepository ?? throw new ArgumentNullException(nameof(currencyRepository));
		_categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
		_systemRepository = systemRepository ?? throw new ArgumentNullException(nameof(systemRepository));
		_unitRepository = unitRepository ?? throw new ArgumentNullException(nameof(unitRepository));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// ????? ?????? ???? - ???? ????? ????????
	/// POST /api/simple-catalog/setup/auto
	/// </summary>
	[HttpPost("auto")]
	[ProducesResponseType(typeof(SetupResult), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<SetupResult>> AutoSetup(
		[FromBody] AutoSetupRequest? request = null)
	{
		try
		{
			_logger.LogInformation("Starting auto setup");

			var result = new SetupResult();

			// ????? ?????? ??????????
			var existingCurrency = await _currencyRepository.GetAllWithPredicateAsync(
				c => c.IsBaseCurrency,
				includeDeleted: false);

			if (existingCurrency.Count == 0)
			{
				var currency = new Currency
				{
					Name = request?.CurrencyName ?? "Saudi Riyal",
					Code = request?.CurrencyCode ?? "SAR",
					Symbol = request?.CurrencySymbol ?? "?",
					IsBaseCurrency = true,
					SymbolBeforeAmount = false,
					DecimalPlaces = 2
				};

				var createdCurrency = await _currencyRepository.AddAsync(currency);
				result.Currency = new CurrencyInfo
				{
					Id = createdCurrency.Id,
					Code = createdCurrency.Code,
					Symbol = createdCurrency.Symbol
				};

				_logger.LogInformation("Created default currency: {Code}", currency.Code);
			}
			else
			{
				var currency = existingCurrency.First();
				result.Currency = new CurrencyInfo
				{
					Id = currency.Id,
					Code = currency.Code,
					Symbol = currency.Symbol
				};
			}

			// ????? ???? ??????
			var existingSystem = await _systemRepository.GetAllWithPredicateAsync(
				s => s.IsDefault,
				includeDeleted: false);

			MeasurementSystem system;
			if (existingSystem.Count == 0)
			{
				system = new MeasurementSystem
				{
					Name = "Default System",
					Code = "default",
					IsDefault = true
				};
				system = await _systemRepository.AddAsync(system);
				_logger.LogInformation("Created default measurement system");
			}
			else
			{
				system = existingSystem.First();
			}

			// ????? ??? ?????? ?????????? (Piece/Unit)
			var existingCategory = await _categoryRepository.GetAllWithPredicateAsync(
				c => c.Code == "piece",
				includeDeleted: false);

			UnitCategory category;
			if (existingCategory.Count == 0)
			{
				category = new UnitCategory
				{
					Name = "Piece",
					Code = "piece",
					Description = "Countable items"
				};
				category = await _categoryRepository.AddAsync(category);
				_logger.LogInformation("Created default unit category");
			}
			else
			{
				category = existingCategory.First();
			}

			// ????? ?????? ??????????
			var existingUnit = await _unitRepository.GetAllWithPredicateAsync(
				u => u.Symbol == "pc",
				includeDeleted: false);

			if (existingUnit.Count == 0)
			{
				var unit = new Unit
				{
					Name = "Piece",
					Symbol = "pc",
					//Code = "piece",
					UnitCategoryId = category.Id,
					MeasurementSystemId = system.Id,
					ConversionToBase = 1,
					IsStandard = true
				};

				var createdUnit = await _unitRepository.AddAsync(unit);
				result.Unit = new UnitInfo
				{
					Id = createdUnit.Id,
					Name = createdUnit.Name,
					Symbol = createdUnit.Symbol
				};

				_logger.LogInformation("Created default unit: Piece");
			}
			else
			{
				var unit = existingUnit.First();
				result.Unit = new UnitInfo
				{
					Id = unit.Id,
					Name = unit.Name,
					Symbol = unit.Symbol
				};
			}

			result.IsSuccess = true;
			result.Message = "Setup completed successfully";

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during auto setup");
			return StatusCode(500, new
			{
				message = "An error occurred during setup",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ?? ???????
	/// GET /api/simple-catalog/setup/check
	/// </summary>
	[HttpGet("check")]
	[ProducesResponseType(typeof(SetupStatus), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<SetupStatus>> CheckSetup()
	{
		try
		{
			var status = new SetupStatus();

			var currencies = await _currencyRepository.GetAllWithPredicateAsync(
				c => c.IsBaseCurrency,
				includeDeleted: false);
			status.HasDefaultCurrency = currencies.Count > 0;

			var systems = await _systemRepository.GetAllWithPredicateAsync(
				s => s.IsDefault,
				includeDeleted: false);
			status.HasDefaultSystem = systems.Count > 0;

			var units = await _unitRepository.ListAllAsync();
			status.HasDefaultUnit = units.Count > 0;

			status.IsReady = status.HasDefaultCurrency &&
						   status.HasDefaultSystem &&
						   status.HasDefaultUnit;

			return Ok(status);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error checking setup");
			return StatusCode(500, new
			{
				message = "An error occurred while checking setup",
				detail = ex.Message
			});
		}
	}
}

// DTOs
public class AutoSetupRequest
{
	public string? CurrencyName { get; set; }
	public string? CurrencyCode { get; set; }
	public string? CurrencySymbol { get; set; }
}

public class SetupResult
{
	public bool IsSuccess { get; set; }
	public string Message { get; set; } = string.Empty;
	public CurrencyInfo? Currency { get; set; }
	public UnitInfo? Unit { get; set; }
}

public class CurrencyInfo
{
	public Guid Id { get; set; }
	public string Code { get; set; } = string.Empty;
	public string Symbol { get; set; } = string.Empty;
}

public class UnitInfo
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Symbol { get; set; } = string.Empty;
}

public class SetupStatus
{
	public bool IsReady { get; set; }
	public bool HasDefaultCurrency { get; set; }
	public bool HasDefaultSystem { get; set; }
	public bool HasDefaultUnit { get; set; }
}


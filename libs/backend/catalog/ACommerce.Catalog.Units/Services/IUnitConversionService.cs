using ACommerce.Catalog.Units.Entities;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Repositories;

namespace ACommerce.Catalog.Units.Services;

/// <summary>
/// ???? ????? ???????
/// </summary>
public interface IUnitConversionService
{
	/// <summary>
	/// ????? ?? ???? ??? ????
	/// </summary>
	Task<decimal> ConvertAsync(
		decimal value,
		Unit fromUnit,
		Unit toUnit,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ????? ?? ???? ??? ???? ?????? ????????
	/// </summary>
	Task<decimal> ConvertAsync(
		decimal value,
		Guid fromUnitId,
		Guid toUnitId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ?????? ??? ????? ??????? ??? ??????
	/// </summary>
	Task<decimal> GetConversionFactorAsync(
		Unit fromUnit,
		Unit toUnit,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ?????? ?? ??????? ??????? ??? ??????
	/// </summary>
	Task<bool> CanConvertAsync(
		Unit fromUnit,
		Unit toUnit,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ?????? ??? ???? ??????? ????????? ???????
	/// </summary>
	Task<List<Unit>> GetCompatibleUnitsAsync(
		Unit unit,
		CancellationToken cancellationToken = default);
}

/// <summary>
/// ????? ???? ????? ???????
/// </summary>
public class UnitConversionService : IUnitConversionService
{
	private readonly IBaseAsyncRepository<Unit> _unitRepository;
	private readonly IBaseAsyncRepository<UnitConversion> _conversionRepository;
	private readonly ILogger<UnitConversionService> _logger;

	public UnitConversionService(
		IBaseAsyncRepository<Unit> unitRepository,
		IBaseAsyncRepository<UnitConversion> conversionRepository,
		ILogger<UnitConversionService> logger)
	{
		_unitRepository = unitRepository ?? throw new ArgumentNullException(nameof(unitRepository));
		_conversionRepository = conversionRepository ?? throw new ArgumentNullException(nameof(conversionRepository));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<decimal> ConvertAsync(
		decimal value,
		Unit fromUnit,
		Unit toUnit,
		CancellationToken cancellationToken = default)
	{
		// ??? ???????
		if (fromUnit.Id == toUnit.Id)
			return value;

		// ?????? ?? ??? ?????
		if (fromUnit.UnitCategoryId != toUnit.UnitCategoryId)
		{
			throw new InvalidOperationException(
				$"Cannot convert between different unit categories");
		}

		_logger.LogDebug(
			"Converting {Value} from {FromUnit} to {ToUnit}",
			value,
			fromUnit.Symbol,
			toUnit.Symbol);

		// 1. ?????? ??????? ??????? ????? (????)
		var directConversion = await GetDirectConversionAsync(
			fromUnit.Id,
			toUnit.Id,
			cancellationToken);

		if (directConversion != null)
		{
			_logger.LogDebug("Using direct conversion");
			var result = ApplyConversion(value, directConversion);
			return Math.Round(result, toUnit.DecimalPlaces);
		}

		// 2. ??????? ??? ?????? ????????
		_logger.LogDebug("Using base unit conversion");

		// 2.1 ????? ??? ?????? ????????
		var baseValue = ConvertToBase(value, fromUnit);

		// 2.2 ????? ?? ?????? ???????? ??? ?????? ?????????
		var finalValue = ConvertFromBase(baseValue, toUnit);

		return Math.Round(finalValue, toUnit.DecimalPlaces);
	}

	public async Task<decimal> ConvertAsync(
		decimal value,
		Guid fromUnitId,
		Guid toUnitId,
		CancellationToken cancellationToken = default)
	{
		var fromUnit = await _unitRepository.GetByIdAsync(fromUnitId, cancellationToken);
		var toUnit = await _unitRepository.GetByIdAsync(toUnitId, cancellationToken);

		if (fromUnit == null)
			throw new ArgumentException($"Unit with id {fromUnitId} not found", nameof(fromUnitId));

		if (toUnit == null)
			throw new ArgumentException($"Unit with id {toUnitId} not found", nameof(toUnitId));

		return await ConvertAsync(value, fromUnit, toUnit, cancellationToken);
	}

	public async Task<decimal> GetConversionFactorAsync(
		Unit fromUnit,
		Unit toUnit,
		CancellationToken cancellationToken = default)
	{
		return await ConvertAsync(1, fromUnit, toUnit, cancellationToken);
	}

	public async Task<bool> CanConvertAsync(
		Unit fromUnit,
		Unit toUnit,
		CancellationToken cancellationToken = default)
	{
		// ???? ??????? ??? ???? ??? ?????
		return fromUnit.UnitCategoryId == toUnit.UnitCategoryId;
	}

	public async Task<List<Unit>> GetCompatibleUnitsAsync(
		Unit unit,
		CancellationToken cancellationToken = default)
	{
		var units = await _unitRepository.GetAllWithPredicateAsync(
			u => u.UnitCategoryId == unit.UnitCategoryId && u.Id != unit.Id,
			includeDeleted: false);

		return units.ToList();
	}

	// ====================================================================================
	// Private Helper Methods
	// ====================================================================================

	private async Task<UnitConversion?> GetDirectConversionAsync(
		Guid fromUnitId,
		Guid toUnitId,
		CancellationToken cancellationToken)
	{
		var conversions = await _conversionRepository.GetAllWithPredicateAsync(
			c => c.FromUnitId == fromUnitId && c.ToUnitId == toUnitId,
			includeDeleted: false);

		return conversions
			.OrderByDescending(c => c.Priority)
			.FirstOrDefault();
	}

	private decimal ConvertToBase(decimal value, Unit unit)
	{
		if (!string.IsNullOrEmpty(unit.ConversionFormula))
		{
			// ??????? ????????
			return EvaluateFormula(unit.ConversionFormula, value);
		}

		// ??????? ????? ???????
		return value * unit.ConversionToBase;
	}

	private decimal ConvertFromBase(decimal baseValue, Unit unit)
	{
		if (!string.IsNullOrEmpty(unit.ConversionFormula))
		{
			// ??????? ???????? ??????? (TODO: implement reverse formula)
			throw new NotImplementedException("Reverse formula conversion not implemented yet");
		}

		// ??????? ????? ???????
		return baseValue / unit.ConversionToBase;
	}

	private decimal ApplyConversion(decimal value, UnitConversion conversion)
	{
		if (!string.IsNullOrEmpty(conversion.Formula))
		{
			return EvaluateFormula(conversion.Formula, value);
		}

		return value * conversion.ConversionFactor;
	}

	private decimal EvaluateFormula(string formula, decimal value)
	{
		// TODO: ??????? ????? ?????? ????????? (??? NCalc ?? DynamicExpresso)
		// ??? ???? ????:
		try
		{
			var expression = formula.Replace("value", value.ToString(System.Globalization.CultureInfo.InvariantCulture));
			// ??????? NCalc ?? ????? ????
			throw new NotImplementedException("Formula evaluation not implemented yet. Use NCalc library.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error evaluating formula {Formula}", formula);
			throw new InvalidOperationException($"Invalid conversion formula: {formula}", ex);
		}
	}
}


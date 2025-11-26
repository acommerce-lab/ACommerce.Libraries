using ACommerce.Catalog.Currencies.Entities;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.Abstractions.Repositories;
using System.Globalization;

namespace ACommerce.Catalog.Currencies.Services;

/// <summary>
/// ????? ???? ????? ???????
/// </summary>
public class CurrencyConversionService : ICurrencyConversionService
{
	private readonly IBaseAsyncRepository<Currency> _currencyRepository;
	private readonly IBaseAsyncRepository<ExchangeRate> _exchangeRateRepository;
	private readonly ILogger<CurrencyConversionService> _logger;

	public CurrencyConversionService(
		IBaseAsyncRepository<Currency> currencyRepository,
		IBaseAsyncRepository<ExchangeRate> exchangeRateRepository,
		ILogger<CurrencyConversionService> logger)
	{
		_currencyRepository = currencyRepository ?? throw new ArgumentNullException(nameof(currencyRepository));
		_exchangeRateRepository = exchangeRateRepository ?? throw new ArgumentNullException(nameof(exchangeRateRepository));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<decimal> ConvertAsync(
		decimal amount,
		Currency fromCurrency,
		Currency toCurrency,
		DateTime? date = null,
		string? rateType = null,
		CancellationToken cancellationToken = default)
	{
		// ??? ???????
		if (fromCurrency.Id == toCurrency.Id)
			return amount;

		date ??= DateTime.UtcNow;

		_logger.LogDebug(
			"Converting {Amount} from {FromCurrency} to {ToCurrency} on {Date}",
			amount,
			fromCurrency.Code,
			toCurrency.Code,
			date);

		// ?????? ??? ??? ?????
		var rate = await GetExchangeRateAsync(
			fromCurrency,
			toCurrency,
			date,
			rateType,
			cancellationToken);

		var convertedAmount = amount * rate;

		// ????? ??? ??? ??????? ??????? ?????? ?????
		convertedAmount = Math.Round(convertedAmount, toCurrency.DecimalPlaces);

		_logger.LogInformation(
			"Converted {Amount} {FromCode} to {ConvertedAmount} {ToCode} at rate {Rate}",
			amount,
			fromCurrency.Code,
			convertedAmount,
			toCurrency.Code,
			rate);

		return convertedAmount;
	}

	public async Task<decimal> ConvertAsync(
		decimal amount,
		Guid fromCurrencyId,
		Guid toCurrencyId,
		DateTime? date = null,
		string? rateType = null,
		CancellationToken cancellationToken = default)
	{
		var fromCurrency = await _currencyRepository.GetByIdAsync(fromCurrencyId, cancellationToken) as Currency;
		var toCurrency = await _currencyRepository.GetByIdAsync(toCurrencyId, cancellationToken) as Currency;

		if (fromCurrency == null)
			throw new ArgumentException($"Currency with id {fromCurrencyId} not found", nameof(fromCurrencyId));

		if (toCurrency == null)
			throw new ArgumentException($"Currency with id {toCurrencyId} not found", nameof(toCurrencyId));

		return await ConvertAsync(amount, fromCurrency, toCurrency, date, rateType, cancellationToken);
	}

	public async Task<decimal> GetExchangeRateAsync(
		Currency fromCurrency,
		Currency toCurrency,
		DateTime? date = null,
		string? rateType = null,
		CancellationToken cancellationToken = default)
	{
		date ??= DateTime.UtcNow;
		rateType ??= "official";

		_logger.LogDebug(
			"Getting exchange rate from {FromCurrency} to {ToCurrency} on {Date}",
			fromCurrency.Code,
			toCurrency.Code,
			date);

		// ????? ?? ??? ?????
		var filters = new List<FilterItem>
		{
			new()
			{
				PropertyName = nameof(ExchangeRate.FromCurrencyId),
				Value = fromCurrency.Id,
				Operator = FilterOperator.Equals
			},
			new()
			{
				PropertyName = nameof(ExchangeRate.ToCurrencyId),
				Value = toCurrency.Id,
				Operator = FilterOperator.Equals
			},
			new()
			{
				PropertyName = nameof(ExchangeRate.EffectiveFrom),
				Value = date,
				Operator = FilterOperator.LessThanOrEqual
			},
			new()
			{
				PropertyName = nameof(ExchangeRate.RateType),
				Value = rateType,
				Operator = FilterOperator.Equals
			}
		};

		var searchRequest = new SmartSearchRequest
		{
			Filters = filters,
			OrderBy = nameof(ExchangeRate.EffectiveFrom),
			Ascending = false, // ?????? ?????
			PageSize = 1
		};

		var result = await _exchangeRateRepository.SmartSearchAsync(searchRequest, cancellationToken);

		if (result.Items.Count == 0)
		{
			_logger.LogWarning(
				"No exchange rate found from {FromCurrency} to {ToCurrency}",
				fromCurrency.Code,
				toCurrency.Code);

			throw new InvalidOperationException(
				$"No exchange rate found from {fromCurrency.Code} to {toCurrency.Code}");
		}

		var exchangeRate = result.Items[0];

		// ?????? ?? ????? ????????
		if (exchangeRate.EffectiveTo.HasValue && exchangeRate.EffectiveTo.Value < date)
		{
			throw new InvalidOperationException(
				$"Exchange rate from {fromCurrency.Code} to {toCurrency.Code} has expired");
		}

		return exchangeRate.Rate;
	}

	public async Task<List<ExchangeRate>> GetExchangeRateHistoryAsync(
		Currency fromCurrency,
		Currency toCurrency,
		DateTime startDate,
		DateTime endDate,
		string? rateType = null,
		CancellationToken cancellationToken = default)
	{
		rateType ??= "official";

		_logger.LogDebug(
			"Getting exchange rate history from {FromCurrency} to {ToCurrency} between {StartDate} and {EndDate}",
			fromCurrency.Code,
			toCurrency.Code,
			startDate,
			endDate);

		var filters = new List<FilterItem>
		{
			new()
			{
				PropertyName = nameof(ExchangeRate.FromCurrencyId),
				Value = fromCurrency.Id,
				Operator = FilterOperator.Equals
			},
			new()
			{
				PropertyName = nameof(ExchangeRate.ToCurrencyId),
				Value = toCurrency.Id,
				Operator = FilterOperator.Equals
			},
			new()
			{
				PropertyName = nameof(ExchangeRate.EffectiveFrom),
				Value = startDate,
				SecondValue = endDate,
				Operator = FilterOperator.Between
			},
			new()
			{
				PropertyName = nameof(ExchangeRate.RateType),
				Value = rateType,
				Operator = FilterOperator.Equals
			}
		};

		var searchRequest = new SmartSearchRequest
		{
			Filters = filters,
			OrderBy = nameof(ExchangeRate.EffectiveFrom),
			Ascending = true,
			PageSize = 1000 // ?? ???? ???????
		};

		var result = await _exchangeRateRepository.SmartSearchAsync(searchRequest, cancellationToken);

		return result.Items.ToList();
	}

	public async Task<ExchangeRate> UpdateExchangeRateAsync(
		Currency fromCurrency,
		Currency toCurrency,
		decimal rate,
		string? source = null,
		string? rateType = null,
		CancellationToken cancellationToken = default)
	{
		rateType ??= "official";

		_logger.LogInformation(
			"Updating exchange rate from {FromCurrency} to {ToCurrency} to {Rate}",
			fromCurrency.Code,
			toCurrency.Code,
			rate);

		var exchangeRate = new ExchangeRate
		{
			FromCurrencyId = fromCurrency.Id,
			ToCurrencyId = toCurrency.Id,
			Rate = rate,
			EffectiveFrom = DateTime.UtcNow,
			Source = source,
			RateType = rateType,
			Priority = 1
		};

		exchangeRate.CalculateInverseRate();

		var created = await _exchangeRateRepository.AddAsync(exchangeRate, cancellationToken);

		return created;
	}

	public string FormatAmount(decimal amount, Currency currency)
	{
		// ????? ??? ??? ??????? ???????
		amount = Math.Round(amount, currency.DecimalPlaces);

		// ????? ?????
		var numberFormat = new NumberFormatInfo
		{
			NumberDecimalDigits = currency.DecimalPlaces,
			NumberDecimalSeparator = currency.DecimalSeparator,
			NumberGroupSeparator = currency.ThousandsSeparator
		};

		var formattedAmount = amount.ToString("N", numberFormat);

		// ????? ?????
		if (currency.SymbolBeforeAmount)
		{
			return $"{currency.Symbol}{formattedAmount}";
		}
		else
		{
			return $"{formattedAmount} {currency.Symbol}";
		}
	}
}


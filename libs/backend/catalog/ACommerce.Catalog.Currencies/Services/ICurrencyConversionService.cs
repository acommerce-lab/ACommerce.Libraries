using ACommerce.Catalog.Currencies.Entities;

namespace ACommerce.Catalog.Currencies.Services;

/// <summary>
/// ???? ????? ???????
/// </summary>
public interface ICurrencyConversionService
{
	/// <summary>
	/// ????? ?? ???? ??? ????
	/// </summary>
	Task<decimal> ConvertAsync(
		decimal amount,
		Currency fromCurrency,
		Currency toCurrency,
		DateTime? date = null,
		string? rateType = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ????? ?? ???? ??? ???? ?????? ????????
	/// </summary>
	Task<decimal> ConvertAsync(
		decimal amount,
		Guid fromCurrencyId,
		Guid toCurrencyId,
		DateTime? date = null,
		string? rateType = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ?????? ??? ??? ????? ??????
	/// </summary>
	Task<decimal> GetExchangeRateAsync(
		Currency fromCurrency,
		Currency toCurrency,
		DateTime? date = null,
		string? rateType = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ?????? ??? ????? ????? ?????
	/// </summary>
	Task<List<ExchangeRate>> GetExchangeRateHistoryAsync(
		Currency fromCurrency,
		Currency toCurrency,
		DateTime startDate,
		DateTime endDate,
		string? rateType = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ????? ??? ???
	/// </summary>
	Task<ExchangeRate> UpdateExchangeRateAsync(
		Currency fromCurrency,
		Currency toCurrency,
		decimal rate,
		string? source = null,
		string? rateType = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// ????? ???? ????? ??? ??????
	/// </summary>
	string FormatAmount(decimal amount, Currency currency);
}


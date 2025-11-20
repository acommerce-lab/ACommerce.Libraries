using ACommerce.Catalog.Currencies.DTOs.ExchangeRate;
using FluentValidation;

namespace ACommerce.Catalog.Currencies.Validators;

public class CreateExchangeRateValidator : AbstractValidator<CreateExchangeRateDto>
{
	public CreateExchangeRateValidator()
	{
		RuleFor(x => x.FromCurrencyId)
			.NotEmpty().WithMessage("From currency is required");

		RuleFor(x => x.ToCurrencyId)
			.NotEmpty().WithMessage("To currency is required");

		RuleFor(x => x.Rate)
			.GreaterThan(0).WithMessage("Exchange rate must be greater than 0");

		RuleFor(x => x.ExpiryDate)
			.GreaterThan(x => x.EffectiveDate ?? DateTime.UtcNow)
			.When(x => x.ExpiryDate.HasValue)
			.WithMessage("Expiry date must be after effective date");
	}
}


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

		RuleFor(x => x.EffectiveFrom)
			.NotEmpty().WithMessage("Effective from date is required");

		RuleFor(x => x.EffectiveTo)
			.GreaterThan(x => x.EffectiveFrom)
			.When(x => x.EffectiveTo.HasValue)
			.WithMessage("Effective to date must be after effective from date");
	}
}


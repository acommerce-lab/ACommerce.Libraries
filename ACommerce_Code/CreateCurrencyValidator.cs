using ACommerce.Catalog.Currencies.DTOs;
using ACommerce.Catalog.Currencies.DTOs.Currency;
using FluentValidation;

namespace ACommerce.Catalog.Currencies.Validators;

public class CreateCurrencyValidator : AbstractValidator<CreateCurrencyDto>
{
	public CreateCurrencyValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Name is required")
			.MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

		RuleFor(x => x.CurrencyCode)
			.NotEmpty().WithMessage("Currency code is required")
			.Length(3).WithMessage("Currency code must be exactly 3 characters (ISO 4217)")
			.Matches("^[A-Z]{3}$").WithMessage("Currency code must be 3 uppercase letters (e.g., SAR, USD)");

		RuleFor(x => x.Symbol)
			.NotEmpty().WithMessage("Symbol is required")
			.MaximumLength(10).WithMessage("Symbol cannot exceed 10 characters");

		RuleFor(x => x.DecimalPlaces)
			.GreaterThanOrEqualTo(0).WithMessage("Decimal places must be 0 or greater")
			.LessThanOrEqualTo(4).WithMessage("Decimal places cannot exceed 4");
	}
}


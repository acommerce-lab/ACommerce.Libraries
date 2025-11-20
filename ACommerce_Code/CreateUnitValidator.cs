using ACommerce.Catalog.Units.DTOs.Unit;
using FluentValidation;

namespace ACommerce.Catalog.Units.Validators;

public class CreateUnitValidator : AbstractValidator<CreateUnitDto>
{
	public CreateUnitValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Name is required")
			.MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

		RuleFor(x => x.Symbol)
			.NotEmpty().WithMessage("Symbol is required")
			.MaximumLength(20).WithMessage("Symbol cannot exceed 20 characters");

		RuleFor(x => x.Code)
			.NotEmpty().WithMessage("Code is required")
			.MaximumLength(50).WithMessage("Code cannot exceed 50 characters")
			.Matches("^[a-z0-9_]+$").WithMessage("Code must be lowercase alphanumeric with underscores only");

		RuleFor(x => x.MeasurementCategoryId)
			.NotEmpty().WithMessage("MeasurementCategoryId is required");

		RuleFor(x => x.MeasurementSystemId)
			.NotEmpty().WithMessage("MeasurementSystemId is required");

		RuleFor(x => x.ConversionToBase)
			.GreaterThan(0).WithMessage("ConversionToBase must be greater than 0");

		RuleFor(x => x.DecimalPlaces)
			.GreaterThanOrEqualTo(0).WithMessage("DecimalPlaces must be 0 or greater")
			.LessThanOrEqualTo(10).WithMessage("DecimalPlaces cannot exceed 10");
	}
}


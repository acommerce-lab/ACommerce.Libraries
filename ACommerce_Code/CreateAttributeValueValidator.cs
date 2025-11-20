using ACommerce.Catalog.Attributes.DTOs.AttributeValue;
using FluentValidation;

namespace ACommerce.Catalog.Attributes.Validators;

public class CreateAttributeValueValidator : AbstractValidator<CreateAttributeValueDto>
{
	public CreateAttributeValueValidator()
	{
		RuleFor(x => x.AttributeDefinitionId)
			.NotEmpty().WithMessage("AttributeDefinitionId is required");

		RuleFor(x => x.Value)
			.NotEmpty().WithMessage("Value is required")
			.MaximumLength(500).WithMessage("Value cannot exceed 500 characters");

		RuleFor(x => x.DisplayName)
			.MaximumLength(200).When(x => !string.IsNullOrEmpty(x.DisplayName))
			.WithMessage("DisplayName cannot exceed 200 characters");

		RuleFor(x => x.ColorHex)
			.Matches("^#[0-9A-Fa-f]{6}$").When(x => !string.IsNullOrEmpty(x.ColorHex))
			.WithMessage("ColorHex must be a valid hex color (e.g., #FF5733)");
	}
}


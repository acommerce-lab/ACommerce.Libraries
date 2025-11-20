using ACommerce.Catalog.Attributes.DTOs.AttributeDefinition;
using FluentValidation;

namespace ACommerce.Catalog.Attributes.Validators;

public class CreateAttributeDefinitionValidator : AbstractValidator<CreateAttributeDefinitionDto>
{
	public CreateAttributeDefinitionValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Name is required")
			.MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

		RuleFor(x => x.Code)
			.NotEmpty().WithMessage("Code is required")
			.MaximumLength(100).WithMessage("Code cannot exceed 100 characters")
			.Matches("^[a-z0-9_]+$").WithMessage("Code must be lowercase alphanumeric with underscores only");

		RuleFor(x => x.Type)
			.IsInEnum().WithMessage("Invalid attribute type");

		RuleFor(x => x.Description)
			.MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Description))
			.WithMessage("Description cannot exceed 1000 characters");
	}
}


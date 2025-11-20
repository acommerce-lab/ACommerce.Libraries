using ACommerce.Transactions.Core.DTOs.DocumentType;
using FluentValidation;

namespace ACommerce.Transactions.Core.Validators;

public class CreateDocumentTypeValidator : AbstractValidator<CreateDocumentTypeDto>
{
	public CreateDocumentTypeValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Name is required")
			.MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

		RuleFor(x => x.Code)
			.NotEmpty().WithMessage("Code is required")
			.MaximumLength(50).WithMessage("Code cannot exceed 50 characters")
			.Matches("^[a-z0-9_]+$").WithMessage("Code must be lowercase alphanumeric with underscores only");

		RuleFor(x => x.Category)
			.IsInEnum().WithMessage("Invalid document category");

		RuleFor(x => x.NumberPrefix)
			.MaximumLength(10).When(x => !string.IsNullOrEmpty(x.NumberPrefix))
			.WithMessage("Number prefix cannot exceed 10 characters");

		RuleFor(x => x.NumberLength)
			.GreaterThan(0).WithMessage("Number length must be greater than 0")
			.LessThanOrEqualTo(20).WithMessage("Number length cannot exceed 20");

		RuleFor(x => x.ColorHex)
			.Matches("^#[0-9A-Fa-f]{6}$").When(x => !string.IsNullOrEmpty(x.ColorHex))
			.WithMessage("ColorHex must be a valid hex color (e.g., #FF5733)");
	}
}


using ACommerce.Catalog.Products.DTOs.Product;
using FluentValidation;

namespace ACommerce.Catalog.Products.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
	public CreateProductValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Product name is required")
			.MaximumLength(500).WithMessage("Product name cannot exceed 500 characters");

		RuleFor(x => x.Sku)
			.NotEmpty().WithMessage("SKU is required")
			.MaximumLength(100).WithMessage("SKU cannot exceed 100 characters")
			.Matches("^[a-zA-Z0-9-_]+$").WithMessage("SKU can only contain alphanumeric characters, hyphens, and underscores");

		RuleFor(x => x.Type)
			.IsInEnum().WithMessage("Invalid product type");

		RuleFor(x => x.Status)
			.IsInEnum().WithMessage("Invalid product status");

		RuleFor(x => x.Weight)
			.GreaterThanOrEqualTo(0).When(x => x.Weight.HasValue)
			.WithMessage("Weight must be greater than or equal to 0");

		RuleFor(x => x.Length)
			.GreaterThanOrEqualTo(0).When(x => x.Length.HasValue)
			.WithMessage("Length must be greater than or equal to 0");

		RuleFor(x => x.Width)
			.GreaterThanOrEqualTo(0).When(x => x.Width.HasValue)
			.WithMessage("Width must be greater than or equal to 0");

		RuleFor(x => x.Height)
			.GreaterThanOrEqualTo(0).When(x => x.Height.HasValue)
			.WithMessage("Height must be greater than or equal to 0");

		RuleFor(x => x.ShortDescription)
			.MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.ShortDescription))
			.WithMessage("Short description cannot exceed 1000 characters");

		RuleFor(x => x.NewUntil)
			.GreaterThan(DateTime.UtcNow).When(x => x.NewUntil.HasValue)
			.WithMessage("NewUntil date must be in the future");
	}
}


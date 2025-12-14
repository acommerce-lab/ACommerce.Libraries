using ACommerce.Catalog.Products.DTOs.ProductCategory;
using FluentValidation;

namespace ACommerce.Catalog.Products.Validators;

public class CreateProductCategoryValidator : AbstractValidator<CreateProductCategoryDto>
{
	public CreateProductCategoryValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Category name is required")
			.MaximumLength(200).WithMessage("Category name cannot exceed 200 characters");

		RuleFor(x => x.Slug)
			.NotEmpty().WithMessage("Slug is required")
			.MaximumLength(200).WithMessage("Slug cannot exceed 200 characters")
			.Matches("^[a-z0-9-]+$").WithMessage("Slug can only contain lowercase letters, numbers, and hyphens");

		RuleFor(x => x.Description)
			.MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Description))
			.WithMessage("Description cannot exceed 2000 characters");
	}
}


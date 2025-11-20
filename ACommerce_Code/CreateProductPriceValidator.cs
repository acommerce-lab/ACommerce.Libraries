using ACommerce.Catalog.Products.DTOs.ProductPrice;
using FluentValidation;

namespace ACommerce.Catalog.Products.Validators;

public class CreateProductPriceValidator : AbstractValidator<CreateProductPriceDto>
{
	public CreateProductPriceValidator()
	{
		RuleFor(x => x.ProductId)
			.NotEmpty().WithMessage("Product ID is required");

		RuleFor(x => x.CurrencyId)
			.NotEmpty().WithMessage("Currency ID is required");

		RuleFor(x => x.BasePrice)
			.GreaterThan(0).WithMessage("Base price must be greater than 0");

		RuleFor(x => x.SalePrice)
			.GreaterThan(0).When(x => x.SalePrice.HasValue)
			.WithMessage("Sale price must be greater than 0")
			.LessThan(x => x.BasePrice).When(x => x.SalePrice.HasValue)
			.WithMessage("Sale price must be less than base price");

		RuleFor(x => x.DiscountPercentage)
			.GreaterThanOrEqualTo(0).When(x => x.DiscountPercentage.HasValue)
			.WithMessage("Discount percentage must be 0 or greater")
			.LessThanOrEqualTo(100).When(x => x.DiscountPercentage.HasValue)
			.WithMessage("Discount percentage cannot exceed 100");

		RuleFor(x => x.SaleEndDate)
			.GreaterThan(x => x.SaleStartDate ?? DateTime.UtcNow)
			.When(x => x.SaleEndDate.HasValue)
			.WithMessage("Sale end date must be after sale start date");

		RuleFor(x => x.MinQuantity)
			.GreaterThan(0).WithMessage("Minimum quantity must be greater than 0");

		RuleFor(x => x.MaxQuantity)
			.GreaterThan(x => x.MinQuantity).When(x => x.MaxQuantity.HasValue)
			.WithMessage("Maximum quantity must be greater than minimum quantity");
	}
}


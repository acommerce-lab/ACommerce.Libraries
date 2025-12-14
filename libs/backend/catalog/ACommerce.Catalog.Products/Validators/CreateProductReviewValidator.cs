using ACommerce.Catalog.Products.DTOs.ProductReview;
using FluentValidation;

namespace ACommerce.Catalog.Products.Validators;

public class CreateProductReviewValidator : AbstractValidator<CreateProductReviewDto>
{
	public CreateProductReviewValidator()
	{
		RuleFor(x => x.ProductId)
			.NotEmpty().WithMessage("Product ID is required");

		RuleFor(x => x.UserId)
			.NotEmpty().WithMessage("User ID is required");

		RuleFor(x => x.UserName)
			.NotEmpty().WithMessage("User name is required")
			.MaximumLength(200).WithMessage("User name cannot exceed 200 characters");

		RuleFor(x => x.Rating)
			.InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

		RuleFor(x => x.Title)
			.MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Title))
			.WithMessage("Title cannot exceed 200 characters");

		RuleFor(x => x.Comment)
			.MaximumLength(5000).When(x => !string.IsNullOrEmpty(x.Comment))
			.WithMessage("Comment cannot exceed 5000 characters");
	}
}


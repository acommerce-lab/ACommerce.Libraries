using ACommerce.Spaces.DTOs.Review;
using FluentValidation;

namespace ACommerce.Spaces.Validators;

public class CreateReviewValidator : AbstractValidator<CreateReviewDto>
{
    public CreateReviewValidator()
    {
        RuleFor(x => x.SpaceId)
            .NotEmpty().WithMessage("معرف المساحة مطلوب");

        RuleFor(x => x.ReviewerId)
            .NotEmpty().WithMessage("معرف المقيّم مطلوب");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("التقييم يجب أن يكون بين 1 و 5");

        RuleFor(x => x.CleanlinessRating)
            .InclusiveBetween(1, 5).When(x => x.CleanlinessRating.HasValue)
            .WithMessage("تقييم النظافة يجب أن يكون بين 1 و 5");

        RuleFor(x => x.LocationRating)
            .InclusiveBetween(1, 5).When(x => x.LocationRating.HasValue)
            .WithMessage("تقييم الموقع يجب أن يكون بين 1 و 5");

        RuleFor(x => x.AmenitiesRating)
            .InclusiveBetween(1, 5).When(x => x.AmenitiesRating.HasValue)
            .WithMessage("تقييم المرافق يجب أن يكون بين 1 و 5");

        RuleFor(x => x.ValueRating)
            .InclusiveBetween(1, 5).When(x => x.ValueRating.HasValue)
            .WithMessage("تقييم القيمة يجب أن يكون بين 1 و 5");

        RuleFor(x => x.CommunicationRating)
            .InclusiveBetween(1, 5).When(x => x.CommunicationRating.HasValue)
            .WithMessage("تقييم التواصل يجب أن يكون بين 1 و 5");

        RuleFor(x => x.Title)
            .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage("عنوان التقييم يجب أن لا يتجاوز 200 حرف");

        RuleFor(x => x.Comment)
            .MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Comment))
            .WithMessage("نص التقييم يجب أن لا يتجاوز 2000 حرف");

        RuleFor(x => x.Pros)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Pros))
            .WithMessage("الإيجابيات يجب أن لا تتجاوز 500 حرف");

        RuleFor(x => x.Cons)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Cons))
            .WithMessage("السلبيات يجب أن لا تتجاوز 500 حرف");
    }
}

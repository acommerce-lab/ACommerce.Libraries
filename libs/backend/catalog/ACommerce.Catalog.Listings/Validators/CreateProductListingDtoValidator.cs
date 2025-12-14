using ACommerce.Catalog.Listings.DTOs;
using FluentValidation;

namespace ACommerce.Catalog.Listings.Validators;

public class CreateProductListingDtoValidator : AbstractValidator<CreateProductListingDto>
{
    private const int MaxImageUrlLength = 2048;
    private const string Base64Prefix = "data:";
    
    public CreateProductListingDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("العنوان مطلوب")
            .MaximumLength(500).WithMessage("العنوان طويل جداً");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("السعر يجب أن يكون أكبر من صفر");

        RuleFor(x => x.FeaturedImage)
            .Must(BeValidImageUrl)
            .When(x => !string.IsNullOrEmpty(x.FeaturedImage))
            .WithMessage("الصورة المميزة يجب أن تكون رابط URL صالح (لا يُسمح بـ base64)");

        RuleForEach(x => x.Images)
            .Must(BeValidImageUrl)
            .When(x => x.Images != null && x.Images.Any())
            .WithMessage("جميع الصور يجب أن تكون روابط URL صالحة (لا يُسمح بـ base64)");
    }

    private static bool BeValidImageUrl(string? imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return true;

        if (imageUrl.StartsWith(Base64Prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        if (imageUrl.Length > MaxImageUrlLength)
            return false;

        return Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) 
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}

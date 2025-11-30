using ACommerce.Spaces.DTOs.Space;
using FluentValidation;

namespace ACommerce.Spaces.Validators;

public class CreateSpaceValidator : AbstractValidator<CreateSpaceDto>
{
    public CreateSpaceValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المساحة مطلوب")
            .MaximumLength(200).WithMessage("اسم المساحة يجب أن لا يتجاوز 200 حرف");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("الرابط المختصر مطلوب")
            .MaximumLength(200).WithMessage("الرابط المختصر يجب أن لا يتجاوز 200 حرف")
            .Matches(@"^[a-z0-9\-]+$").WithMessage("الرابط المختصر يجب أن يحتوي على حروف إنجليزية صغيرة وأرقام وشرطات فقط");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("نوع المساحة غير صالح");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("حالة المساحة غير صالحة");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("السعة يجب أن تكون أكبر من صفر");

        RuleFor(x => x.MinCapacity)
            .GreaterThan(0).WithMessage("الحد الأدنى للسعة يجب أن يكون أكبر من صفر")
            .LessThanOrEqualTo(x => x.Capacity).WithMessage("الحد الأدنى للسعة يجب أن يكون أقل من أو يساوي السعة القصوى");

        RuleFor(x => x.AreaSquareMeters)
            .GreaterThan(0).When(x => x.AreaSquareMeters.HasValue)
            .WithMessage("المساحة يجب أن تكون أكبر من صفر");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("معرف المالك مطلوب");

        RuleFor(x => x.MinBookingDurationMinutes)
            .GreaterThanOrEqualTo(15).WithMessage("الحد الأدنى لمدة الحجز يجب أن يكون 15 دقيقة على الأقل");

        RuleFor(x => x.MaxBookingDurationMinutes)
            .GreaterThan(x => x.MinBookingDurationMinutes)
            .When(x => x.MaxBookingDurationMinutes.HasValue)
            .WithMessage("الحد الأقصى لمدة الحجز يجب أن يكون أكبر من الحد الأدنى");

        RuleFor(x => x.AdvanceNoticeHours)
            .GreaterThanOrEqualTo(0).WithMessage("وقت الإشعار المسبق يجب أن يكون صفر أو أكثر");

        RuleFor(x => x.FreeCancellationHours)
            .GreaterThanOrEqualTo(0).When(x => x.FreeCancellationHours.HasValue)
            .WithMessage("ساعات الإلغاء المجاني يجب أن تكون صفر أو أكثر");

        RuleFor(x => x.ContactEmail)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.ContactEmail))
            .WithMessage("البريد الإلكتروني غير صالح");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue)
            .WithMessage("خط العرض يجب أن يكون بين -90 و 90");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue)
            .WithMessage("خط الطول يجب أن يكون بين -180 و 180");

        RuleFor(x => x.ShortDescription)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.ShortDescription))
            .WithMessage("الوصف المختصر يجب أن لا يتجاوز 500 حرف");

        RuleFor(x => x.LongDescription)
            .MaximumLength(5000).When(x => !string.IsNullOrEmpty(x.LongDescription))
            .WithMessage("الوصف التفصيلي يجب أن لا يتجاوز 5000 حرف");
    }
}

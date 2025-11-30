using ACommerce.Spaces.DTOs.Booking;
using FluentValidation;

namespace ACommerce.Spaces.Validators;

public class CreateBookingValidator : AbstractValidator<CreateBookingDto>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.SpaceId)
            .NotEmpty().WithMessage("معرف المساحة مطلوب");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("معرف العميل مطلوب");

        RuleFor(x => x.StartDateTime)
            .NotEmpty().WithMessage("تاريخ ووقت البداية مطلوب")
            .GreaterThan(DateTime.UtcNow).WithMessage("تاريخ البداية يجب أن يكون في المستقبل");

        RuleFor(x => x.EndDateTime)
            .NotEmpty().WithMessage("تاريخ ووقت النهاية مطلوب")
            .GreaterThan(x => x.StartDateTime).WithMessage("تاريخ النهاية يجب أن يكون بعد تاريخ البداية");

        RuleFor(x => x.GuestsCount)
            .GreaterThan(0).WithMessage("عدد الضيوف يجب أن يكون أكبر من صفر");

        RuleFor(x => x.PricingType)
            .IsInEnum().WithMessage("نوع التسعير غير صالح");

        RuleFor(x => x.CustomerEmail)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.CustomerEmail))
            .WithMessage("البريد الإلكتروني غير صالح");

        RuleFor(x => x.CustomerPhone)
            .Matches(@"^[\+]?[0-9\s\-\(\)]+$").When(x => !string.IsNullOrEmpty(x.CustomerPhone))
            .WithMessage("رقم الهاتف غير صالح");

        RuleFor(x => x.Purpose)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Purpose))
            .WithMessage("غرض الحجز يجب أن لا يتجاوز 500 حرف");

        RuleFor(x => x.CustomerNotes)
            .MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.CustomerNotes))
            .WithMessage("ملاحظات العميل يجب أن لا تتجاوز 1000 حرف");

        RuleFor(x => x.SpecialRequests)
            .MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.SpecialRequests))
            .WithMessage("الطلبات الخاصة يجب أن لا تتجاوز 1000 حرف");
    }
}

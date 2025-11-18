using ACommerce.Notifications.Recipients.DTOs.ContactPoint;
using ACommerce.Notifications.Recipients.Enums;
using FluentValidation;

namespace ACommerce.Notifications.Recipients.Validators;

/// <summary>
/// ?????? ?? ??? ????? ???? ?????
/// </summary>
public class CreateContactPointValidator : AbstractValidator<CreateContactPointDto>
{
	public CreateContactPointValidator()
	{
		RuleFor(x => x.UserId)
			.NotEmpty()
			.WithMessage("UserId is required")
			.MaximumLength(450)
			.WithMessage("UserId cannot exceed 450 characters");

		RuleFor(x => x.Value)
			.NotEmpty()
			.WithMessage("Value is required")
			.MaximumLength(500)
			.WithMessage("Value cannot exceed 500 characters");

		RuleFor(x => x.Value)
			.EmailAddress()
			.When(x => x.Type == ContactPointType.Email)
			.WithMessage("Invalid email format");

		RuleFor(x => x.Value)
			.Matches(@"^\+?[1-9]\d{1,14}$")
			.When(x => x.Type == ContactPointType.PhoneNumber)
			.WithMessage("Invalid phone number format (use E.164 format: +966501234567)");

		RuleFor(x => x.Type)
			.IsInEnum()
			.WithMessage("Invalid contact point type");
	}
}


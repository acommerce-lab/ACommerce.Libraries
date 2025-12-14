using ACommerce.Notifications.Recipients.DTOs.UserRecipient;
using FluentValidation;

namespace ACommerce.Notifications.Recipients.Validators;

/// <summary>
/// ?????? ?? ??? ????? ?????
/// </summary>
public class CreateUserRecipientValidator : AbstractValidator<CreateUserRecipientDto>
{
	public CreateUserRecipientValidator()
	{
		RuleFor(x => x.UserId)
			.NotEmpty()
			.WithMessage("UserId is required")
			.MaximumLength(450)
			.WithMessage("UserId cannot exceed 450 characters");

		RuleFor(x => x.FullName)
			.MaximumLength(200)
			.When(x => !string.IsNullOrEmpty(x.FullName))
			.WithMessage("FullName cannot exceed 200 characters");

		RuleFor(x => x.PreferredLanguage)
			.NotEmpty()
			.WithMessage("PreferredLanguage is required")
			.Length(2)
			.WithMessage("PreferredLanguage must be ISO 639-1 code (2 characters)")
			.Matches("^[a-z]{2}$")
			.WithMessage("PreferredLanguage must be lowercase ISO 639-1 code (e.g., ar, en)");

		RuleFor(x => x.TimeZone)
			.NotEmpty()
			.WithMessage("TimeZone is required");

		RuleForEach(x => x.ContactPoints)
			.SetValidator(new CreateContactPointValidator())
			.When(x => x.ContactPoints != null);
	}
}


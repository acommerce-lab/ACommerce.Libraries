using ACommerce.Notifications.Recipients.DTOs.RecipientGroup;
using FluentValidation;

namespace ACommerce.Notifications.Recipients.Validators;

/// <summary>
/// ?????? ?? ??? ????? ??????
/// </summary>
public class CreateRecipientGroupValidator : AbstractValidator<CreateRecipientGroupDto>
{
	public CreateRecipientGroupValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage("Name is required")
			.MaximumLength(200)
			.WithMessage("Name cannot exceed 200 characters");

		RuleFor(x => x.Description)
			.MaximumLength(1000)
			.When(x => !string.IsNullOrEmpty(x.Description))
			.WithMessage("Description cannot exceed 1000 characters");
	}
}


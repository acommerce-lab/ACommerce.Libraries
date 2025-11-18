// Validators/Users/ConfirmEmailDtoValidator.cs
using ACommerce.Authentication.AspNetCore.DTOs.Users;
using FluentValidation;

namespace ACommerce.Authentication.AspNetCore.Validators.Users;

// Validators/Users/ConfirmEmailDtoValidator.cs
/// <summary>
/// ?????? ?? ??? ????? ?????? ??????????
/// </summary>
public class ConfirmEmailDtoValidator : AbstractValidator<ConfirmEmailDto>
{
	public ConfirmEmailDtoValidator()
	{
		RuleFor(x => x.UserId)
			.NotEmpty()
			.WithMessage("User ID is required");

		RuleFor(x => x.Token)
			.NotEmpty()
			.WithMessage("Confirmation token is required");
	}
}


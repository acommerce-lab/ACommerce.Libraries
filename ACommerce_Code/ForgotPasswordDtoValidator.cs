// Validators/Users/ForgotPasswordDtoValidator.cs
using ACommerce.Authentication.AspNetCore.DTOs.Users;
using FluentValidation;

namespace ACommerce.Authentication.AspNetCore.Validators.Users;

// Validators/Users/ForgotPasswordDtoValidator.cs
/// <summary>
/// ?????? ?? ??? ??? ????? ????? ???? ??????
/// </summary>
public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
{
	public ForgotPasswordDtoValidator()
	{
		RuleFor(x => x.Identifier)
			.NotEmpty()
			.WithMessage("Email or username is required")
			.MaximumLength(256)
			.WithMessage("Identifier cannot exceed 256 characters");
	}
}


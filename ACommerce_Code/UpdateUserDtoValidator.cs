// Validators/Users/UpdateUserDtoValidator.cs
using ACommerce.Authentication.AspNetCore.DTOs.Users;
using FluentValidation;

namespace ACommerce.Authentication.AspNetCore.Validators.Users;

// Validators/Users/UpdateUserDtoValidator.cs
/// <summary>
/// ?????? ?? ??? ????? ??????
/// </summary>
public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
	public UpdateUserDtoValidator()
	{
		RuleFor(x => x.Username)
			.MinimumLength(3)
			.WithMessage("Username must be at least 3 characters")
			.MaximumLength(50)
			.WithMessage("Username cannot exceed 50 characters")
			.Matches("^[a-zA-Z0-9_-]+$")
			.WithMessage("Username can only contain letters, numbers, hyphens, and underscores")
			.When(x => !string.IsNullOrEmpty(x.Username));

		RuleFor(x => x.Email)
			.EmailAddress()
			.WithMessage("Invalid email format")
			.MaximumLength(256)
			.WithMessage("Email cannot exceed 256 characters")
			.When(x => !string.IsNullOrEmpty(x.Email));

		RuleFor(x => x.PhoneNumber)
			.Matches(@"^\+?[1-9]\d{1,14}$")
			.When(x => !string.IsNullOrEmpty(x.PhoneNumber))
			.WithMessage("Invalid phone number format (use E.164 format: +966501234567)");

		//RuleFor(x => x.PreferredTwoFactorMethod)
		//	.IsInEnum()
		//	.When(x => x.PreferredTwoFactorMethod.HasValue)
		//	.WithMessage("Invalid two-factor method");
	}
}


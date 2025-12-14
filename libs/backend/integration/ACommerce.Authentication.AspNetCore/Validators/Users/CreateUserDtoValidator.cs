// Validators/Users/CreateUserDtoValidator.cs
using ACommerce.Authentication.AspNetCore.DTOs.Users;
using FluentValidation;

namespace ACommerce.Authentication.AspNetCore.Validators.Users;

/// <summary>
/// ?????? ?? ??? ????? ??????
/// </summary>
public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
	public CreateUserDtoValidator()
	{
		RuleFor(x => x.Username)
			.NotEmpty()
			.WithMessage("Username is required")
			.MinimumLength(3)
			.WithMessage("Username must be at least 3 characters")
			.MaximumLength(50)
			.WithMessage("Username cannot exceed 50 characters")
			.Matches("^[a-zA-Z0-9_-]+$")
			.WithMessage("Username can only contain letters, numbers, hyphens, and underscores");

		RuleFor(x => x.Email)
			.NotEmpty()
			.WithMessage("Email is required")
			.EmailAddress()
			.WithMessage("Invalid email format")
			.MaximumLength(256)
			.WithMessage("Email cannot exceed 256 characters");

		RuleFor(x => x.PhoneNumber)
			.Matches(@"^\+?[1-9]\d{1,14}$")
			.When(x => !string.IsNullOrEmpty(x.PhoneNumber))
			.WithMessage("Invalid phone number format (use E.164 format: +966501234567)");

		RuleFor(x => x.Password)
			.NotEmpty()
			.WithMessage("Password is required")
			.MinimumLength(8)
			.WithMessage("Password must be at least 8 characters")
			.Matches("[A-Z]")
			.WithMessage("Password must contain at least one uppercase letter")
			.Matches("[a-z]")
			.WithMessage("Password must contain at least one lowercase letter")
			.Matches("[0-9]")
			.WithMessage("Password must contain at least one digit")
			.Matches("[^a-zA-Z0-9]")
			.WithMessage("Password must contain at least one special character");

		RuleFor(x => x.ConfirmPassword)
			.Equal(x => x.Password)
			.WithMessage("Password and confirmation do not match");

		//RuleFor(x => x.PreferredTwoFactorMethod)
		//	.IsInEnum()
		//	.When(x => x.TwoFactorEnabled && x.PreferredTwoFactorMethod.HasValue)
		//	.WithMessage("Invalid two-factor method");
	}
}


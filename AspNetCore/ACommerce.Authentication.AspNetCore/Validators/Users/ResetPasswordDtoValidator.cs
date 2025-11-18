// Validators/Users/ResetPasswordDtoValidator.cs
using ACommerce.Authentication.AspNetCore.DTOs.Users;
using FluentValidation;

namespace ACommerce.Authentication.AspNetCore.Validators.Users;

// Validators/Users/ResetPasswordDtoValidator.cs
/// <summary>
/// ?????? ?? ??? ????? ????? ???? ??????
/// </summary>
public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
	public ResetPasswordDtoValidator()
	{
		RuleFor(x => x.Token)
			.NotEmpty()
			.WithMessage("Reset token is required");

		RuleFor(x => x.NewPassword)
			.NotEmpty()
			.WithMessage("New password is required")
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

		RuleFor(x => x.ConfirmNewPassword)
			.Equal(x => x.NewPassword)
			.WithMessage("Password and confirmation do not match");
	}
}


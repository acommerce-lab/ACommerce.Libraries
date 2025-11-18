// Validators/Roles/AddUserToRoleDtoValidator.cs
using ACommerce.Authentication.AspNetCore.DTOs.Roles;
using FluentValidation;

namespace ACommerce.Authentication.AspNetCore.Validators.Roles;

// Validators/Roles/AddUserToRoleDtoValidator.cs
/// <summary>
/// ?????? ?? ??? ????? ?????? ????
/// </summary>
public class AddUserToRoleDtoValidator : AbstractValidator<AddUserToRoleDto>
{
	public AddUserToRoleDtoValidator()
	{
		RuleFor(x => x.UserId)
			.NotEmpty()
			.WithMessage("User ID is required");

		//RuleFor(x => x.ExpiresAt)
		//	.GreaterThan(DateTimeOffset.UtcNow)
		//	.When(x => x.ExpiresAt.HasValue)
		//	.WithMessage("Expiration date must be in the future");
	}
}


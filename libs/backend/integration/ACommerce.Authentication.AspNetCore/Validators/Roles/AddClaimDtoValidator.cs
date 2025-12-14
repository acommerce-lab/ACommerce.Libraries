// Validators/Roles/AddClaimDtoValidator.cs
using ACommerce.Authentication.AspNetCore.DTOs.Claims;
using FluentValidation;

namespace ACommerce.Authentication.AspNetCore.Validators.Roles;

// Validators/Claims/AddClaimDtoValidator.cs
/// <summary>
/// ?????? ?? ??? ????? Claim
/// </summary>
public class AddClaimDtoValidator : AbstractValidator<AddClaimDto>
{
	public AddClaimDtoValidator()
	{
		RuleFor(x => x.ClaimType)
			.NotEmpty()
			.WithMessage("Claim type is required")
			.MaximumLength(200)
			.WithMessage("Claim type cannot exceed 200 characters");

		RuleFor(x => x.ClaimValue)
			.NotEmpty()
			.WithMessage("Claim value is required")
			.MaximumLength(500)
			.WithMessage("Claim value cannot exceed 500 characters");
	}
}


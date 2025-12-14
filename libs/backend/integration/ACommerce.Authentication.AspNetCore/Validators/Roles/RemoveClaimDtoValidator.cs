// Validators/Roles/RemoveClaimDtoValidator.cs
using ACommerce.Authentication.AspNetCore.DTOs.Claims;
using FluentValidation;

namespace ACommerce.Authentication.AspNetCore.Validators.Roles;

// Validators/Claims/RemoveClaimDtoValidator.cs
/// <summary>
/// ?????? ?? ??? ????? Claim
/// </summary>
public class RemoveClaimDtoValidator : AbstractValidator<RemoveClaimDto>
{
	public RemoveClaimDtoValidator()
	{
		RuleFor(x => x.ClaimType)
			.NotEmpty()
			.WithMessage("Claim type is required");

		RuleFor(x => x.ClaimValue)
			.NotEmpty()
			.WithMessage("Claim value is required");
	}
}


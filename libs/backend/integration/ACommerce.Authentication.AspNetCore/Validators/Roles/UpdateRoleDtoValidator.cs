// Validators/Roles/UpdateRoleDtoValidator.cs
using ACommerce.Authentication.AspNetCore.DTOs.Roles;
using FluentValidation;

namespace ACommerce.Authentication.AspNetCore.Validators.Roles;

// Validators/Roles/UpdateRoleDtoValidator.cs
/// <summary>
/// ?????? ?? ??? ????? ???
/// </summary>
public class UpdateRoleDtoValidator : AbstractValidator<UpdateRoleDto>
{
	public UpdateRoleDtoValidator()
	{
		RuleFor(x => x.Name)
			.MinimumLength(2)
			.WithMessage("Role name must be at least 2 characters")
			.MaximumLength(100)
			.WithMessage("Role name cannot exceed 100 characters")
			.Matches("^[a-zA-Z0-9_-]+$")
			.WithMessage("Role name can only contain letters, numbers, hyphens, and underscores")
			.When(x => !string.IsNullOrEmpty(x.Name));

		RuleFor(x => x.Description)
			.MaximumLength(500)
			.When(x => !string.IsNullOrEmpty(x.Description))
			.WithMessage("Description cannot exceed 500 characters");
	}
}


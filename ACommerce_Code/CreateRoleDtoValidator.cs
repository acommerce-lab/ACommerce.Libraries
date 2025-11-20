// Validators/Roles/CreateRoleDtoValidator.cs
using ACommerce.Authentication.AspNetCore.DTOs.Roles;
using FluentValidation;

namespace ACommerce.Authentication.AspNetCore.Validators.Roles;

/// <summary>
/// ?????? ?? ??? ????? ???
/// </summary>
public class CreateRoleDtoValidator : AbstractValidator<CreateRoleDto>
{
	public CreateRoleDtoValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage("Role name is required")
			.MinimumLength(2)
			.WithMessage("Role name must be at least 2 characters")
			.MaximumLength(100)
			.WithMessage("Role name cannot exceed 100 characters")
			.Matches("^[a-zA-Z0-9_-]+$")
			.WithMessage("Role name can only contain letters, numbers, hyphens, and underscores");

		RuleFor(x => x.Description)
			.MaximumLength(500)
			.When(x => !string.IsNullOrEmpty(x.Description))
			.WithMessage("Description cannot exceed 500 characters");
	}
}


using ACommerce.Accounting.Core.DTOs.Account;
using FluentValidation;

namespace ACommerce.Accounting.Core.Validators;

public class CreateAccountValidator : AbstractValidator<CreateAccountDto>
{
	public CreateAccountValidator()
	{
		RuleFor(x => x.ChartOfAccountsId)
			.NotEmpty().WithMessage("Chart of accounts is required");

		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Account name is required")
			.MaximumLength(200).WithMessage("Account name cannot exceed 200 characters");

		RuleFor(x => x.Code)
			.NotEmpty().WithMessage("Account code is required")
			.MaximumLength(50).WithMessage("Account code cannot exceed 50 characters");

		RuleFor(x => x.Type)
			.IsInEnum().WithMessage("Invalid account type");

		RuleFor(x => x.Nature)
			.IsInEnum().WithMessage("Invalid account nature");
	}
}


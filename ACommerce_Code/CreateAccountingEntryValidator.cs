using ACommerce.Accounting.Core.DTOs.AccountingEntry;
using FluentValidation;

namespace ACommerce.Accounting.Core.Validators;

public class CreateAccountingEntryValidator : AbstractValidator<CreateAccountingEntryDto>
{
	public CreateAccountingEntryValidator()
	{
		RuleFor(x => x.Number)
			.NotEmpty().WithMessage("Entry number is required")
			.MaximumLength(50).WithMessage("Entry number cannot exceed 50 characters");

		RuleFor(x => x.Description)
			.NotEmpty().WithMessage("Description is required")
			.MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

		RuleFor(x => x.Date)
			.NotEmpty().WithMessage("Entry date is required");

		RuleFor(x => x.FiscalYear)
			.GreaterThan(1900).WithMessage("Fiscal year must be greater than 1900")
			.LessThanOrEqualTo(2100).WithMessage("Fiscal year must be less than or equal to 2100");

		RuleFor(x => x.FiscalPeriod)
			.InclusiveBetween(1, 12).WithMessage("Fiscal period must be between 1 and 12");

		RuleFor(x => x.Sides)
			.NotEmpty().WithMessage("Entry must have at least 2 sides")
			.Must(sides => sides.Count >= 2).WithMessage("Entry must have at least 2 sides");

		RuleFor(x => x)
			.Must(IsBalanced).WithMessage("Entry must be balanced (Total Debits = Total Credits)");
	}

	private bool IsBalanced(CreateAccountingEntryDto entry)
	{
		var totalDebit = entry.Sides.Sum(s => s.DebitAmount ?? 0);
		var totalCredit = entry.Sides.Sum(s => s.CreditAmount ?? 0);

		return Math.Abs(totalDebit - totalCredit) < 0.01m;
	}
}


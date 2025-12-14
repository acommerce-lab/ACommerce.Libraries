using ACommerce.Accounting.Core.DTOs.EntrySide;
using FluentValidation;

namespace ACommerce.Accounting.Core.Validators;

public class CreateEntrySideValidator : AbstractValidator<CreateEntrySideDto>
{
	public CreateEntrySideValidator()
	{
		RuleFor(x => x.AccountId)
			.NotEmpty().WithMessage("Account is required");

		RuleFor(x => x.LineNumber)
			.GreaterThan(0).WithMessage("Line number must be greater than 0");

		RuleFor(x => x)
			.Must(HasEitherDebitOrCredit)
			.WithMessage("Entry side must have either debit or credit amount");

		RuleFor(x => x)
			.Must(NotHaveBothDebitAndCredit)
			.WithMessage("Entry side cannot have both debit and credit amounts");

		RuleFor(x => x.DebitAmount)
			.GreaterThan(0).When(x => x.DebitAmount.HasValue)
			.WithMessage("Debit amount must be greater than 0");

		RuleFor(x => x.CreditAmount)
			.GreaterThan(0).When(x => x.CreditAmount.HasValue)
			.WithMessage("Credit amount must be greater than 0");

		RuleFor(x => x.DebitQuantity)
			.GreaterThan(0).When(x => x.DebitQuantity.HasValue)
			.WithMessage("Debit quantity must be greater than 0");

		RuleFor(x => x.CreditQuantity)
			.GreaterThan(0).When(x => x.CreditQuantity.HasValue)
			.WithMessage("Credit quantity must be greater than 0");

		RuleFor(x => x.ExchangeRate)
			.GreaterThan(0).When(x => x.ExchangeRate.HasValue)
			.WithMessage("Exchange rate must be greater than 0");
	}

	private bool HasEitherDebitOrCredit(CreateEntrySideDto side)
	{
		return side.DebitAmount.HasValue || side.CreditAmount.HasValue;
	}

	private bool NotHaveBothDebitAndCredit(CreateEntrySideDto side)
	{
		return !(side.DebitAmount.HasValue && side.CreditAmount.HasValue);
	}
}


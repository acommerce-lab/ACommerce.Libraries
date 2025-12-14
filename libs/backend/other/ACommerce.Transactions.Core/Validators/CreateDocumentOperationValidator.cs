using ACommerce.Transactions.Core.DTOs.DocumentOperation;
using FluentValidation;

namespace ACommerce.Transactions.Core.Validators;

public class CreateDocumentOperationValidator : AbstractValidator<CreateDocumentOperationDto>
{
	public CreateDocumentOperationValidator()
	{
		RuleFor(x => x.DocumentTypeId)
			.NotEmpty().WithMessage("Document type ID is required");

		RuleFor(x => x.Operation)
			.IsInEnum().WithMessage("Invalid operation type");

		RuleFor(x => x.CustomName)
			.MaximumLength(100).When(x => !string.IsNullOrEmpty(x.CustomName))
			.WithMessage("Custom name cannot exceed 100 characters");

		RuleFor(x => x.ApprovalRoles)
			.NotEmpty().When(x => x.RequiresApproval)
			.WithMessage("Approval roles are required when operation requires approval");
	}
}


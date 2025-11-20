using ACommerce.Transactions.Core.Enums;

namespace ACommerce.Transactions.Core.DTOs.DocumentOperation;

public class CreateDocumentOperationDto
{
	public Guid DocumentTypeId { get; set; }
	public OperationType Operation { get; set; }
	public string? CustomName { get; set; }
	public string? Description { get; set; }
	public bool RequiresApproval { get; set; }
	public List<string>? AllowedRoles { get; set; }
	public List<string>? ApprovalRoles { get; set; }
	public string? Conditions { get; set; }
	public int SortOrder { get; set; }
	public string? Icon { get; set; }
	public string? ColorHex { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}


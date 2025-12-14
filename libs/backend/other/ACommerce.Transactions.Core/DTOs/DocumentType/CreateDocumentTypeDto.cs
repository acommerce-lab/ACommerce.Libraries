using ACommerce.Transactions.Core.Enums;

namespace ACommerce.Transactions.Core.DTOs.DocumentType;

public class CreateDocumentTypeDto
{
	public required string Name { get; set; }
	public required string Code { get; set; }
	public DocumentCategory Category { get; set; }
	public string? Description { get; set; }
	public string? Icon { get; set; }
	public string? ColorHex { get; set; }
	public string? NumberPrefix { get; set; }
	public int NumberLength { get; set; } = 6;
	public bool RequiresApproval { get; set; }
	public bool AffectsInventory { get; set; }
	public bool AffectsAccounting { get; set; } = true;
	public int SortOrder { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}


using ACommerce.Transactions.Core.Enums;

namespace ACommerce.Transactions.Core.DTOs.DocumentType;

public class DocumentTypeResponseDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Code { get; set; } = string.Empty;
	public DocumentCategory Category { get; set; }
	public string? Description { get; set; }
	public string? Icon { get; set; }
	public string? ColorHex { get; set; }
	public string? NumberPrefix { get; set; }
	public int NumberLength { get; set; }
	public bool RequiresApproval { get; set; }
	public bool AffectsInventory { get; set; }
	public bool AffectsAccounting { get; set; }
	public bool IsActive { get; set; }
	public int SortOrder { get; set; }
	public int OperationsCount { get; set; }
	public int AttributesCount { get; set; }
	public Dictionary<string, string> Metadata { get; set; } = new();
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}


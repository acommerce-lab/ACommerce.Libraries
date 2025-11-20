using ACommerce.Transactions.Core.Enums;

namespace ACommerce.Transactions.Core.DTOs.DocumentTypeRelation;

public class CreateDocumentTypeRelationDto
{
	public Guid SourceDocumentTypeId { get; set; }
	public Guid TargetDocumentTypeId { get; set; }
	public DocumentRelationType RelationType { get; set; }
	public bool IsRequired { get; set; }
	public bool AllowMultiple { get; set; }
	public string? Conditions { get; set; }
	public int Priority { get; set; } = 1;
	public Dictionary<string, string>? Metadata { get; set; }
}


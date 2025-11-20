using ACommerce.Accounting.Core.DTOs.EntrySide;
using ACommerce.Accounting.Core.Enums;

namespace ACommerce.Accounting.Core.DTOs.AccountingEntry;

public class CreateAccountingEntryDto
{
	public required string Number { get; set; }
	public EntryType Type { get; set; } = EntryType.Manual;
	public DateTime Date { get; set; }
	public required string Description { get; set; }
	public Guid? SourceDocumentId { get; set; }
	public Guid? SourceDocumentTypeId { get; set; }
	public string? SourceDocumentNumber { get; set; }
	public Guid? SourceDocumentItemId { get; set; }
	public int FiscalYear { get; set; }
	public int FiscalPeriod { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
	public List<CreateEntrySideDto> Sides { get; set; } = [];
}


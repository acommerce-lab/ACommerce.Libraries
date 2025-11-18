using ACommerce.Accounting.Core.DTOs.EntrySide;
using ACommerce.Accounting.Core.Enums;

namespace ACommerce.Accounting.Core.DTOs.AccountingEntry;

public class AccountingEntryResponseDto
{
	public Guid Id { get; set; }
	public string Number { get; set; } = string.Empty;
	public EntryType Type { get; set; }
	public DateTime Date { get; set; }
	public string Description { get; set; } = string.Empty;
	public Guid? SourceDocumentId { get; set; }
	public Guid? SourceDocumentTypeId { get; set; }
	public string? SourceDocumentNumber { get; set; }
	public EntryStatus Status { get; set; }
	public DateTime? PostedDate { get; set; }
	public string? PostedByUserId { get; set; }
	public DateTime? ApprovedDate { get; set; }
	public string? ApprovedByUserId { get; set; }
	public int FiscalYear { get; set; }
	public int FiscalPeriod { get; set; }
	public bool IsBalanced { get; set; }
	public decimal TotalDebitAmount { get; set; }
	public decimal TotalCreditAmount { get; set; }
	public List<EntrySideResponseDto> Sides { get; set; } = new();
	public Dictionary<string, string> Metadata { get; set; } = new();
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}


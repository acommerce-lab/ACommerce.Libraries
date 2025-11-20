namespace ACommerce.Accounting.Core.DTOs.EntrySide;

public class EntrySideResponseDto
{
	public Guid Id { get; set; }
	public Guid EntryId { get; set; }
	public int LineNumber { get; set; }

	// ??????
	public Guid AccountId { get; set; }
	public string AccountName { get; set; } = string.Empty;
	public string AccountCode { get; set; } = string.Empty;

	// ????? ??????
	public Guid? CurrencyId { get; set; }
	public string? CurrencyCode { get; set; }
	public decimal? DebitAmount { get; set; }
	public decimal? CreditAmount { get; set; }
	public decimal? ExchangeRate { get; set; }
	public decimal? BaseDebitAmount { get; set; }
	public decimal? BaseCreditAmount { get; set; }

	// ????? ?????
	public Guid? UnitId { get; set; }
	public string? UnitName { get; set; }
	public decimal? DebitQuantity { get; set; }
	public decimal? CreditQuantity { get; set; }

	// ???? ???????
	public Guid? CostCenterId { get; set; }
	public string? CostCenterName { get; set; }

	// ??????
	public Guid? ProjectId { get; set; }
	public Guid? DepartmentId { get; set; }

	public string? Description { get; set; }
	public Dictionary<string, string> Metadata { get; set; } = new();
	public DateTime CreatedAt { get; set; }
}


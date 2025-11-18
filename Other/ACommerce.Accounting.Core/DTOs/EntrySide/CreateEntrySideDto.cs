namespace ACommerce.Accounting.Core.DTOs.EntrySide;

public class CreateEntrySideDto
{
	public int LineNumber { get; set; }
	public Guid AccountId { get; set; }

	// ????? ??????
	public Guid? CurrencyId { get; set; }
	public decimal? DebitAmount { get; set; }
	public decimal? CreditAmount { get; set; }
	public decimal? ExchangeRate { get; set; }

	// ????? ?????
	public Guid? UnitId { get; set; }
	public decimal? DebitQuantity { get; set; }
	public decimal? CreditQuantity { get; set; }

	// ????? ???? ???????
	public Guid? CostCenterId { get; set; }

	// ????? ??????
	public Guid? ProjectId { get; set; }
	public Guid? DepartmentId { get; set; }

	public string? Description { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}


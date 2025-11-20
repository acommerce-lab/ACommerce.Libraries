namespace ACommerce.Accounting.Core.DTOs.ChartOfAccounts;

public class CreateChartOfAccountsDto
{
	public required string Name { get; set; }
	public required string Code { get; set; }
	public string? Description { get; set; }
	public Guid? CompanyId { get; set; }
	public Guid? BranchId { get; set; }
	public int? FiscalYear { get; set; }
	public bool IsDefault { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}


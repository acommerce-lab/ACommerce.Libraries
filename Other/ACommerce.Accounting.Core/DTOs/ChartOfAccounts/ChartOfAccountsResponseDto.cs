namespace ACommerce.Accounting.Core.DTOs.ChartOfAccounts;

public class ChartOfAccountsResponseDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Code { get; set; } = string.Empty;
	public string? Description { get; set; }
	public Guid? CompanyId { get; set; }
	public Guid? BranchId { get; set; }
	public int? FiscalYear { get; set; }
	public bool IsDefault { get; set; }
	public bool IsActive { get; set; }
	public int AccountsCount { get; set; }
	public Dictionary<string, string> Metadata { get; set; } = new();
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}


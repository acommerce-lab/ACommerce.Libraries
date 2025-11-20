using ACommerce.Accounting.Core.Enums;

namespace ACommerce.Accounting.Core.DTOs.Account;

public class CreateAccountDto
{
	public Guid ChartOfAccountsId { get; set; }
	public required string Name { get; set; }
	public required string Code { get; set; }
	public AccountType Type { get; set; }
	public AccountNature Nature { get; set; }
	public string? Description { get; set; }
	public Guid? ParentAccountId { get; set; }
	public bool IsLeaf { get; set; } = true;
	public bool AllowPosting { get; set; } = true;
	public Guid? DefaultCostCenterId { get; set; }
	public bool RequiresCostCenter { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}


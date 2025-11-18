using ACommerce.Accounting.Core.Enums;

namespace ACommerce.Accounting.Core.DTOs.Account;

public class AccountResponseDto
{
	public Guid Id { get; set; }
	public Guid ChartOfAccountsId { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Code { get; set; } = string.Empty;
	public AccountType Type { get; set; }
	public AccountNature Nature { get; set; }
	public string? Description { get; set; }
	public Guid? ParentAccountId { get; set; }
	public string? ParentAccountName { get; set; }
	public bool IsLeaf { get; set; }
	public bool AllowPosting { get; set; }
	public int Level { get; set; }
	public Guid? DefaultCostCenterId { get; set; }
	public bool RequiresCostCenter { get; set; }
	public bool IsActive { get; set; }
	public int SubAccountsCount { get; set; }
	public Dictionary<string, string> Metadata { get; set; } = new();
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}


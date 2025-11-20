namespace ACommerce.Accounting.Core.DTOs.CostCenter;

public class CostCenterResponseDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Code { get; set; } = string.Empty;
	public string? Description { get; set; }
	public Guid? ParentCostCenterId { get; set; }
	public string? ParentCostCenterName { get; set; }
	public int Level { get; set; }
	public bool IsActive { get; set; }
	public int SubCostCentersCount { get; set; }
	public Dictionary<string, string> Metadata { get; set; } = new();
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}


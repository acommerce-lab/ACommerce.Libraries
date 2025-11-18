namespace ACommerce.Accounting.Core.DTOs.CostCenter;

public class CreateCostCenterDto
{
	public required string Name { get; set; }
	public required string Code { get; set; }
	public string? Description { get; set; }
	public Guid? ParentCostCenterId { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}


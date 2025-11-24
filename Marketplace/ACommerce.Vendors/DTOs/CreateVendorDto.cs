using ACommerce.Vendors.Enums;

namespace ACommerce.Vendors.DTOs;

public class CreateVendorDto
{
	public Guid ProfileId { get; set; }
	public required string StoreName { get; set; }
	public required string StoreSlug { get; set; }
	public string? Description { get; set; }
	public string? Logo { get; set; }
	public decimal CommissionValue { get; set; }
	public CommissionType CommissionType { get; set; } = CommissionType.Percentage;
	public string? TaxNumber { get; set; }
	public string? CommercialRegister { get; set; }
	public bool AgreedToTerms { get; set; }
}

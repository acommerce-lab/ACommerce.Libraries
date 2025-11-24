using ACommerce.Vendors.Enums;

namespace ACommerce.Vendors.DTOs;

public class VendorResponseDto
{
	public Guid Id { get; set; }
	public Guid ProfileId { get; set; }
	public string StoreName { get; set; } = string.Empty;
	public string StoreSlug { get; set; } = string.Empty;
	public string? Description { get; set; }
	public string? Logo { get; set; }
	public string? BannerImage { get; set; }
	public VendorStatus Status { get; set; }
	public CommissionType CommissionType { get; set; }
	public decimal CommissionValue { get; set; }
	public decimal? Rating { get; set; }
	public int TotalSales { get; set; }
	public decimal AvailableBalance { get; set; }
	public DateTime CreatedAt { get; set; }
}

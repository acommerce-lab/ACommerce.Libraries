namespace ACommerce.Catalog.Products.Services;

/// <summary>
/// ??? ????? ?? ????????
/// </summary>
public class ProductSearchRequest
{
	public string? Query { get; set; }
	public List<Guid>? CategoryIds { get; set; }
	public List<Guid>? BrandIds { get; set; }
	public decimal? MinPrice { get; set; }
	public decimal? MaxPrice { get; set; }
	public List<string>? Tags { get; set; }
	public bool? IsFeatured { get; set; }
	public bool? IsNew { get; set; }
	public int PageNumber { get; set; } = 1;
	public int PageSize { get; set; } = 20;
}


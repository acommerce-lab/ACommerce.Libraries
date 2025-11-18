namespace ACommerce.Catalog.Products.DTOs.ProductCategory;

public class ProductCategoryResponseDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public string? Description { get; set; }
	public string? Image { get; set; }
	public string? Icon { get; set; }
	public Guid? ParentCategoryId { get; set; }
	public string? ParentCategoryName { get; set; }
	public int SubCategoriesCount { get; set; }
	public int ProductsCount { get; set; }
	public int SortOrder { get; set; }
	public bool IsActive { get; set; }
	public Dictionary<string, string> Metadata { get; set; } = new();
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}


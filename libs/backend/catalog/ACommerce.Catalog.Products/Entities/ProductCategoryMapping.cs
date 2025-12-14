using ACommerce.SharedKernel.Abstractions.Entities;

namespace ACommerce.Catalog.Products.Entities;

/// <summary>
/// ??? ?????? ?????? (Many-to-Many)
/// </summary>
public class ProductCategoryMapping : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	public Guid ProductId { get; set; }
	public Product? Product { get; set; }

	public Guid CategoryId { get; set; }
	public ProductCategory? Category { get; set; }

	/// <summary>
	/// ?? ????? ???????? ???????
	/// </summary>
	public bool IsPrimary { get; set; }

	/// <summary>
	/// ????? ?????? ?? ?????
	/// </summary>
	public int SortOrder { get; set; }
}


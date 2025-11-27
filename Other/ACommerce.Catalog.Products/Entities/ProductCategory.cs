using ACommerce.SharedKernel.Abstractions.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACommerce.Catalog.Products.Entities;

/// <summary>
/// ??? ?????? (Category)
/// </summary>
public class ProductCategory : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// ??? ?????
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// ?????
	/// </summary>
	public required string Slug { get; set; }

	/// <summary>
	/// ?????
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// ??????
	/// </summary>
	public string? Image { get; set; }

	/// <summary>
	/// ????????
	/// </summary>
	public string? Icon { get; set; }

	/// <summary>
	/// ????? ????
	/// </summary>
	public Guid? ParentCategoryId { get; set; }
	public ProductCategory? ParentCategory { get; set; }

	/// <summary>
	/// ?????? ???????
	/// </summary>
	public List<ProductCategory> SubCategories { get; set; } = new();

	/// <summary>
	/// ????? ?????
	/// </summary>
	public int SortOrder { get; set; }

	/// <summary>
	/// ?? ?????
	/// </summary>
	public bool IsActive { get; set; } = true;

    /// <summary>
    /// ??????? ??????
    /// </summary>
    [NotMapped] public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// ???????? ?? ??? ?????
    /// </summary>
    public List<ProductCategoryMapping> Products { get; set; } = new();
}


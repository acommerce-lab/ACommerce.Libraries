using System.ComponentModel.DataAnnotations.Schema;
using ACommerce.SharedKernel.Abstractions.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACommerce.Catalog.Products.Entities;

/// <summary>
/// ??????? ????????
/// </summary>
public class ProductBrand : IBaseEntity
{
	public Guid Id { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }

	/// <summary>
	/// ??? ??????? ????????
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
	public string? Logo { get; set; }

	/// <summary>
	/// ?????? ??????????
	/// </summary>
	public string? Website { get; set; }

	/// <summary>
	/// ??????
	/// </summary>
	public string? Country { get; set; }

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
	[NotMapped]
	public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// ???????? ???? ???????
    /// </summary>
    public List<ProductBrandMapping> Products { get; set; } = new();
}


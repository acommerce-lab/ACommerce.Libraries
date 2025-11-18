namespace ACommerce.Catalog.Simple.Api.DTOs;

/// <summary>
/// ???? ????
/// </summary>
public class SimpleProductDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Sku { get; set; } = string.Empty;
	public string? Description { get; set; }
	public decimal Price { get; set; }
	public decimal? SalePrice { get; set; }
	public string? Image { get; set; }
	public List<string> Images { get; set; } = new();
	public Guid CategoryId { get; set; }
	public string CategoryName { get; set; } = string.Empty;
	public Dictionary<string, string> Attributes { get; set; } = new();
	public int Stock { get; set; }
	public bool InStock { get; set; }
	public bool IsFeatured { get; set; }
	public bool IsNew { get; set; }
	public DateTime CreatedAt { get; set; }
}

/// <summary>
/// ??? ????? ???? ????
/// </summary>
public class CreateSimpleProductRequest
{
	public required string Name { get; set; }
	public required string Sku { get; set; }
	public string? Description { get; set; }
	public decimal Price { get; set; }
	public decimal? SalePrice { get; set; }
	public Guid CategoryId { get; set; }
	public string? Image { get; set; }
	public List<string>? Images { get; set; }
	public Dictionary<string, string>? Attributes { get; set; }
	public int Stock { get; set; } = 0;
	public bool IsFeatured { get; set; }
	public bool IsNew { get; set; }
}

/// <summary>
/// ??? ????? ???? ????
/// </summary>
public class UpdateSimpleProductRequest
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public decimal? Price { get; set; }
	public decimal? SalePrice { get; set; }
	public string? Image { get; set; }
	public List<string>? Images { get; set; }
	public Dictionary<string, string>? Attributes { get; set; }
	public int? Stock { get; set; }
	public bool? IsFeatured { get; set; }
	public bool? IsNew { get; set; }
}


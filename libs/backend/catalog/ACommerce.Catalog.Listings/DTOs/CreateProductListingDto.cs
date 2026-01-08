using System.Text.Json.Serialization;

namespace ACommerce.Catalog.Listings.DTOs;

public class CreateProductListingDto
{
	public Guid VendorId { get; set; }
	public Guid ProductId { get; set; }
	public Guid? CategoryId { get; set; }
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	public string? VendorSku { get; set; }
	public required decimal Price { get; set; }
	public decimal? CompareAtPrice { get; set; }
	public decimal? Cost { get; set; }
	public Guid? CurrencyId { get; set; }
	public string? Currency { get; set; }

	/// <summary>
	/// الكمية المتوفرة - يقبل StockQuantity أو QuantityAvailable من JSON
	/// </summary>
	[JsonPropertyName("stockQuantity")]
	public int QuantityAvailable { get; set; }
	public int? ProcessingTime { get; set; }
	public string? VendorNotes { get; set; }
	public string? Condition { get; set; }

	/// <summary>
	/// الصور (JSON array or List)
	/// </summary>
	public List<string>? Images { get; set; }
	public string? FeaturedImage { get; set; }

	/// <summary>
	/// الموقع الجغرافي
	/// </summary>
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public string? Address { get; set; }
	public string? City { get; set; }

	/// <summary>
	/// الخصائص الديناميكية
	/// </summary>
	public Dictionary<string, object>? Attributes { get; set; }

	/// <summary>
	/// نسبة العمولة/العربون - يتم تعيينها تلقائياً من باقة المستخدم
	/// لا يجب إرسالها من العميل
	/// </summary>
	[JsonIgnore]
	public decimal? CommissionPercentage { get; set; }
}

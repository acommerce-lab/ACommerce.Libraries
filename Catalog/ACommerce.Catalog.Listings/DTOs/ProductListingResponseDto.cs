using System.Text.Json;
using System.Text.Json.Serialization;
using ACommerce.Catalog.Listings.Enums;

namespace ACommerce.Catalog.Listings.DTOs;

public class ProductListingResponseDto
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	public Guid Id { get; set; }
	public Guid VendorId { get; set; }
	public string? VendorName { get; set; }
	public Guid ProductId { get; set; }
	public string? ProductName { get; set; }
	public Guid? CategoryId { get; set; }
	public string? CategoryName { get; set; }
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	public string? VendorSku { get; set; }
	public ListingStatus Status { get; set; }
	public decimal Price { get; set; }
	public decimal? CompareAtPrice { get; set; }
	public string Currency { get; set; } = "SAR";
	public int QuantityAvailable { get; set; }
	public int QuantityReserved { get; set; }
	public bool IsActive { get; set; }
	public bool IsFeatured { get; set; }
	public bool IsNew { get; set; }
	public int TotalSales { get; set; }
	public decimal AverageRating { get; set; }
	public int RatingsCount { get; set; }
	public int ViewCount { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }

	/// <summary>
	/// الصورة المميزة
	/// </summary>
	public string? FeaturedImage { get; set; }

	/// <summary>
	/// الموقع الجغرافي
	/// </summary>
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public string? Address { get; set; }
	public string? City { get; set; }

	/// <summary>
	/// حالة المنتج
	/// </summary>
	public string? Condition { get; set; }

	/// <summary>
	/// الصور كـ JSON للتخزين
	/// </summary>
	[JsonIgnore]
	public string? ImagesJson { get; set; }

	/// <summary>
	/// الصور كـ List للاستخدام
	/// </summary>
	public List<string> Images
	{
		get
		{
			if (string.IsNullOrEmpty(ImagesJson)) return new List<string>();
			try
			{
				return JsonSerializer.Deserialize<List<string>>(ImagesJson, JsonOptions) ?? new List<string>();
			}
			catch
			{
				return new List<string>();
			}
		}
		set => ImagesJson = value != null ? JsonSerializer.Serialize(value, JsonOptions) : null;
	}

	/// <summary>
	/// الخصائص كـ JSON للتخزين
	/// </summary>
	[JsonIgnore]
	public string? AttributesJson { get; set; }

	/// <summary>
	/// الخصائص الديناميكية كـ Dictionary للاستخدام
	/// </summary>
	public Dictionary<string, object> Attributes
	{
		get
		{
			if (string.IsNullOrEmpty(AttributesJson)) return new Dictionary<string, object>();
			try
			{
				return JsonSerializer.Deserialize<Dictionary<string, object>>(AttributesJson, JsonOptions)
					?? new Dictionary<string, object>();
			}
			catch
			{
				return new Dictionary<string, object>();
			}
		}
		set => AttributesJson = value != null ? JsonSerializer.Serialize(value, JsonOptions) : null;
	}

	/// <summary>
	/// نسبة الخصم محسوبة
	/// </summary>
	public int? DiscountPercentage
	{
		get
		{
			if (CompareAtPrice.HasValue && CompareAtPrice > Price && CompareAtPrice > 0)
			{
				return (int)Math.Round((1 - (Price / CompareAtPrice.Value)) * 100);
			}
			return null;
		}
	}
}

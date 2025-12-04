using ACommerce.Client.Core.Http;

namespace ACommerce.Client.ProductListings;

public sealed class ProductListingsClient(IApiClient httpClient)
{
    private const string ServiceName = "Marketplace";

    public async Task<List<ProductListingDto>?> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await httpClient.GetAsync<List<ProductListingDto>>(ServiceName, "/api/listings", cancellationToken);
	}

	public async Task<ProductListingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await httpClient.GetAsync<ProductListingDto>(ServiceName, $"/api/listings/{id}", cancellationToken);
	}

	public async Task<List<ProductListingDto>?> GetByVendorAsync(Guid vendorId, CancellationToken cancellationToken = default)
	{
		return await httpClient.GetAsync<List<ProductListingDto>>(ServiceName, $"/api/listings/vendor/{vendorId}", cancellationToken);
	}

	/// <summary>
	/// الحصول على العروض المميزة
	/// </summary>
	public async Task<List<ProductListingDto>?> GetFeaturedAsync(int limit = 10, CancellationToken cancellationToken = default)
	{
		return await httpClient.GetAsync<List<ProductListingDto>>(ServiceName, $"/api/listings/featured?limit={limit}", cancellationToken);
	}

	/// <summary>
	/// الحصول على العروض الجديدة
	/// </summary>
	public async Task<List<ProductListingDto>?> GetNewAsync(int limit = 10, CancellationToken cancellationToken = default)
	{
		return await httpClient.GetAsync<List<ProductListingDto>>(ServiceName, $"/api/listings/new?limit={limit}", cancellationToken);
	}

	/// <summary>
	/// الحصول على العروض حسب الفئة
	/// </summary>
	public async Task<List<ProductListingDto>?> GetByCategoryAsync(Guid categoryId, int limit = 20, CancellationToken cancellationToken = default)
	{
		return await httpClient.GetAsync<List<ProductListingDto>>(ServiceName, $"/api/listings/category/{categoryId}?limit={limit}", cancellationToken);
	}

	/// <summary>
	/// البحث في المنتجات المعروضة
	/// </summary>
	public async Task<SearchListingsResponse?> SearchAsync(
		SearchListingsRequest request,
		CancellationToken cancellationToken = default)
	{
		var queryParams = BuildSearchQuery(request);
		return await httpClient.GetAsync<SearchListingsResponse>(
			ServiceName,
			$"/api/listings/search?{queryParams}",
			cancellationToken);
	}

	private static string BuildSearchQuery(SearchListingsRequest request)
	{
		var queryParts = new List<string>();

		if (!string.IsNullOrEmpty(request.Query))
			queryParts.Add($"q={Uri.EscapeDataString(request.Query)}");

		if (request.CategoryId.HasValue)
			queryParts.Add($"categoryId={request.CategoryId}");

		if (request.VendorId.HasValue)
			queryParts.Add($"vendorId={request.VendorId}");

		if (request.MinPrice.HasValue)
			queryParts.Add($"minPrice={request.MinPrice}");

		if (request.MaxPrice.HasValue)
			queryParts.Add($"maxPrice={request.MaxPrice}");

		if (request.MinRating.HasValue)
			queryParts.Add($"minRating={request.MinRating}");

		if (request.InStockOnly)
			queryParts.Add("inStockOnly=true");

		if (request.IsFeatured.HasValue)
			queryParts.Add($"isFeatured={request.IsFeatured}");

		if (!string.IsNullOrEmpty(request.SortBy))
			queryParts.Add($"sortBy={Uri.EscapeDataString(request.SortBy)}");

		if (request.SortDescending)
			queryParts.Add("sortDesc=true");

		queryParts.Add($"page={request.Page}");
		queryParts.Add($"pageSize={request.PageSize}");

		return string.Join("&", queryParts);
	}

	public async Task<ProductListingDto?> CreateAsync(CreateListingRequest request, CancellationToken cancellationToken = default)
	{
		return await httpClient.PostAsync<CreateListingRequest, ProductListingDto>(ServiceName, "/api/listings", request, cancellationToken);
	}

	public async Task<ProductListingDto?> UpdateAsync(Guid id, UpdateListingRequest request, CancellationToken cancellationToken = default)
	{
		return await httpClient.PutAsync<UpdateListingRequest, ProductListingDto>(ServiceName, $"/api/listings/{id}", request, cancellationToken);
	}

	public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
	{
		await httpClient.DeleteAsync(ServiceName, $"/api/listings/{id}", cancellationToken);
	}
}

/// <summary>
/// ProductListing DTO for client-side use
/// </summary>
public sealed class ProductListingDto
{
	public Guid Id { get; set; }
	public Guid VendorId { get; set; }
	public string? VendorName { get; set; }
	public Guid ProductId { get; set; }
	public string? ProductName { get; set; }
	public Guid? CategoryId { get; set; }
	public string? CategoryName { get; set; }

	/// <summary>
	/// عنوان العرض (يمكن أن يكون مختلفاً عن اسم المنتج)
	/// </summary>
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }

	/// <summary>
	/// الصور
	/// </summary>
	public List<string> Images { get; set; } = new();
	public string? FeaturedImage { get; set; }

	/// <summary>
	/// Alias for FeaturedImage (للتوافق)
	/// </summary>
	public string? ImageUrl => FeaturedImage ?? Images.FirstOrDefault();

	/// <summary>
	/// السعر
	/// </summary>
	public decimal Price { get; set; }
	public decimal? CompareAtPrice { get; set; }
	public int? DiscountPercentage { get; set; }
	public string Currency { get; set; } = "SAR";

	/// <summary>
	/// Alias for CompareAtPrice (للتوافق)
	/// </summary>
	public decimal? OriginalPrice => CompareAtPrice;

	/// <summary>
	/// الموقع
	/// </summary>
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
	public string? Address { get; set; }
	public string? City { get; set; }

	/// <summary>
	/// المخزون والحالة
	/// </summary>
	public int StockQuantity { get; set; }
	public string? Condition { get; set; }
	public string Status { get; set; } = "Active";
	public bool IsActive { get; set; } = true;
	public bool IsFeatured { get; set; }
	public bool IsNew { get; set; }

	/// <summary>
	/// التقييم
	/// </summary>
	public decimal AverageRating { get; set; }
	public int RatingsCount { get; set; }
	public int ViewCount { get; set; }

	/// <summary>
	/// الخصائص الديناميكية
	/// </summary>
	public Dictionary<string, object> Attributes { get; set; } = new();

	/// <summary>
	/// Alias for Attributes (للتوافق)
	/// </summary>
	public Dictionary<string, object> Properties => Attributes;

	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}

public sealed class CreateListingRequest
{
	public Guid VendorId { get; set; }
	public Guid ProductId { get; set; }
	public Guid CategoryId { get; set; }
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	public decimal Price { get; set; }
	public string? Currency { get; set; } = "SAR";
	public int StockQuantity { get; set; } = 1;
	public string? Condition { get; set; }
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
	/// الخصائص الديناميكية بناءً على الفئة
	/// Key = AttributeDefinition.Code, Value = القيمة
	/// </summary>
	public Dictionary<string, object> Attributes { get; set; } = new();
}

public sealed class UpdateListingRequest
{
	public decimal? Price { get; set; }
	public int? StockQuantity { get; set; }
	public string? Condition { get; set; }
}

public sealed class SearchListingsRequest
{
	public string? Query { get; set; }
	public Guid? CategoryId { get; set; }
	public Guid? VendorId { get; set; }
	public decimal? MinPrice { get; set; }
	public decimal? MaxPrice { get; set; }
	public int? MinRating { get; set; }
	public bool InStockOnly { get; set; }
	public bool? IsFeatured { get; set; }
	public string? SortBy { get; set; }
	public bool SortDescending { get; set; }
	public int Page { get; set; } = 1;
	/// <summary>
	/// Alias for Page property
	/// </summary>
	public int PageNumber { get => Page; set => Page = value; }
	public int PageSize { get; set; } = 20;
}

public sealed class SearchListingsResponse
{
	public List<ProductListingDto> Items { get; set; } = new();
	public int TotalCount { get; set; }
	public int Page { get; set; }
	public int PageSize { get; set; }
	public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

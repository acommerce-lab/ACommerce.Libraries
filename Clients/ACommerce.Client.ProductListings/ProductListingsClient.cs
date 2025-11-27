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
	public string? ImageUrl { get; set; }
	public decimal Price { get; set; }
	public decimal OriginalPrice { get; set; }
	public int DiscountPercentage { get; set; }
	public string? Currency { get; set; }
	public int StockQuantity { get; set; }
	public string? Condition { get; set; }
	public string? Status { get; set; }
	public bool IsFeatured { get; set; }
	public decimal AverageRating { get; set; }
	public int RatingsCount { get; set; }
	public Dictionary<string, object> Properties { get; set; } = new();
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}

public sealed class CreateListingRequest
{
	public Guid VendorId { get; set; }
	public Guid ProductId { get; set; }
	public decimal Price { get; set; }
	public int StockQuantity { get; set; }
	public string? Condition { get; set; }
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

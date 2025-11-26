using ACommerce.Client.Core.Http;

namespace ACommerce.Client.ProductListings;

public sealed class ProductListingsClient(DynamicHttpClient httpClient)
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
	public decimal Price { get; set; }
	public string? Currency { get; set; }
	public int StockQuantity { get; set; }
	public string? Condition { get; set; }
	public string? Status { get; set; }
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

using ACommerce.Client.Core.Http;
using ACommerce.ProductListings.Entities;

namespace ACommerce.Client.ProductListings;

public sealed class ProductListingsClient
{
	private readonly DynamicHttpClient _httpClient;
	private const string ServiceName = "Marketplace";

	public ProductListingsClient(DynamicHttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<List<ProductListing>?> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<List<ProductListing>>(ServiceName, "/api/listings", cancellationToken);
	}

	public async Task<ProductListing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<ProductListing>(ServiceName, $"/api/listings/{id}", cancellationToken);
	}

	public async Task<List<ProductListing>?> GetByVendorAsync(Guid vendorId, CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<List<ProductListing>>(ServiceName, $"/api/listings/vendor/{vendorId}", cancellationToken);
	}

	public async Task<ProductListing?> CreateAsync(CreateListingRequest request, CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<CreateListingRequest, ProductListing>(ServiceName, "/api/listings", request, cancellationToken);
	}

	public async Task<ProductListing?> UpdateAsync(Guid id, UpdateListingRequest request, CancellationToken cancellationToken = default)
	{
		return await _httpClient.PutAsync<UpdateListingRequest, ProductListing>(ServiceName, $"/api/listings/{id}", request, cancellationToken);
	}

	public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
	{
		await _httpClient.DeleteAsync(ServiceName, $"/api/listings/{id}", cancellationToken);
	}
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

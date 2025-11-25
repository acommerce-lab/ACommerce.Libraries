using ACommerce.Client.Core.Http;
using ACommerce.Vendors.Entities;

namespace ACommerce.Client.Vendors;

public sealed class VendorsClient
{
	private readonly DynamicHttpClient _httpClient;
	private const string ServiceName = "Marketplace";

	public VendorsClient(DynamicHttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<List<Vendor>?> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<List<Vendor>>(ServiceName, "/api/vendors", cancellationToken);
	}

	public async Task<Vendor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<Vendor>(ServiceName, $"/api/vendors/{id}", cancellationToken);
	}

	public async Task<Vendor?> CreateAsync(CreateVendorRequest request, CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<CreateVendorRequest, Vendor>(ServiceName, "/api/vendors", request, cancellationToken);
	}

	public async Task<Vendor?> UpdateAsync(Guid id, UpdateVendorRequest request, CancellationToken cancellationToken = default)
	{
		return await _httpClient.PutAsync<UpdateVendorRequest, Vendor>(ServiceName, $"/api/vendors/{id}", request, cancellationToken);
	}

	public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
	{
		await _httpClient.DeleteAsync(ServiceName, $"/api/vendors/{id}", cancellationToken);
	}
}

public sealed class CreateVendorRequest
{
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public string ContactEmail { get; set; } = string.Empty;
	public string? ContactPhone { get; set; }
}

public sealed class UpdateVendorRequest
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public string? ContactEmail { get; set; }
	public string? ContactPhone { get; set; }
}

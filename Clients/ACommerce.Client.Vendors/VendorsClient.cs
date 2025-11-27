using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Vendors;

public sealed class VendorsClient
{
	private readonly IApiClient _httpClient;
	private const string ServiceName = "Marketplace";

	public VendorsClient(IApiClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<List<VendorDto>?> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<List<VendorDto>>(ServiceName, "/api/vendors", cancellationToken);
	}

	public async Task<VendorDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<VendorDto>(ServiceName, $"/api/vendors/{id}", cancellationToken);
	}

	public async Task<VendorDto?> CreateAsync(CreateVendorRequest request, CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<CreateVendorRequest, VendorDto>(ServiceName, "/api/vendors", request, cancellationToken);
	}

	public async Task<VendorDto?> UpdateAsync(Guid id, UpdateVendorRequest request, CancellationToken cancellationToken = default)
	{
		return await _httpClient.PutAsync<UpdateVendorRequest, VendorDto>(ServiceName, $"/api/vendors/{id}", request, cancellationToken);
	}

	public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
	{
		await _httpClient.DeleteAsync(ServiceName, $"/api/vendors/{id}", cancellationToken);
	}
}

/// <summary>
/// Vendor DTO for client-side use
/// </summary>
public sealed class VendorDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	public string? ContactEmail { get; set; }
	public string? ContactPhone { get; set; }
	public string? LogoUrl { get; set; }
	public string? Status { get; set; }
	public decimal? Rating { get; set; }
	public int? ReviewCount { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
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

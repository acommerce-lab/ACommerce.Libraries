using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Products;

/// <summary>
/// Client للتعامل مع Products
/// </summary>
public sealed class ProductsClient
{
	private readonly IApiClient _httpClient;
	private const string ServiceName = "Marketplace"; // أو "Products" إذا كانت خدمة منفصلة

	public ProductsClient(IApiClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <summary>
	/// الحصول على جميع المنتجات
	/// </summary>
	public async Task<List<ProductDto>?> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<List<ProductDto>>(
			ServiceName,
			"/api/products",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على منتج محدد
	/// </summary>
	public async Task<ProductDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<ProductDto>(
			ServiceName,
			$"/api/products/{id}",
			cancellationToken);
	}

	/// <summary>
	/// إنشاء منتج جديد
	/// </summary>
	public async Task<ProductDto?> CreateAsync(
		CreateProductRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<CreateProductRequest, ProductDto>(
			ServiceName,
			"/api/products",
			request,
			cancellationToken);
	}

	/// <summary>
	/// تحديث منتج
	/// </summary>
	public async Task<ProductDto?> UpdateAsync(
		string id,
		UpdateProductRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PutAsync<UpdateProductRequest, ProductDto>(
			ServiceName,
			$"/api/products/{id}",
			request,
			cancellationToken);
	}

	/// <summary>
	/// حذف منتج
	/// </summary>
	public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
	{
		await _httpClient.DeleteAsync(
			ServiceName,
			$"/api/products/{id}",
			cancellationToken);
	}
}

/// <summary>
/// Product DTO for client-side use
/// </summary>
public sealed class ProductDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Sku { get; set; } = string.Empty;
	public string? ShortDescription { get; set; }
	public string? LongDescription { get; set; }
	public string? FeaturedImage { get; set; }
	public List<string> Images { get; set; } = new();
	public decimal? Price { get; set; }
	public string? Currency { get; set; }
	public int? StockQuantity { get; set; }
	public bool IsFeatured { get; set; }
	public bool IsNew { get; set; }
	public string? Status { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// طلب إنشاء منتج جديد
/// </summary>
public sealed class CreateProductRequest
{
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public decimal Price { get; set; }
	public string Sku { get; set; } = string.Empty;
	public int StockQuantity { get; set; }
}

/// <summary>
/// طلب تحديث منتج
/// </summary>
public sealed class UpdateProductRequest
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public decimal? Price { get; set; }
	public int? StockQuantity { get; set; }
}

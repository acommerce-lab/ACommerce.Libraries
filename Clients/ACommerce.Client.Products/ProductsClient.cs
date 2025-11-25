using ACommerce.Catalog.Products.Entities;
using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Products;

/// <summary>
/// Client للتعامل مع Products
/// </summary>
public sealed class ProductsClient
{
	private readonly DynamicHttpClient _httpClient;
	private const string ServiceName = "Marketplace"; // أو "Products" إذا كانت خدمة منفصلة

	public ProductsClient(DynamicHttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <summary>
	/// الحصول على جميع المنتجات
	/// </summary>
	public async Task<List<Product>?> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<List<Product>>(
			ServiceName,
			"/api/products",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على منتج محدد
	/// </summary>
	public async Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<Product>(
			ServiceName,
			$"/api/products/{id}",
			cancellationToken);
	}

	/// <summary>
	/// إنشاء منتج جديد
	/// </summary>
	public async Task<Product?> CreateAsync(
		CreateProductRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<CreateProductRequest, Product>(
			ServiceName,
			"/api/products",
			request,
			cancellationToken);
	}

	/// <summary>
	/// تحديث منتج
	/// </summary>
	public async Task<Product?> UpdateAsync(
		string id,
		UpdateProductRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PutAsync<UpdateProductRequest, Product>(
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

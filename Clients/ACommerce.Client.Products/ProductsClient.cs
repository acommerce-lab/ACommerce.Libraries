using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Products;

/// <summary>
/// Client للتعامل مع Products
/// </summary>
public sealed class ProductsClient
{
	private readonly IApiClient _httpClient;
	private const string ServiceName = "Marketplace";
	private const string BasePath = "/api/catalog/products";

	public ProductsClient(IApiClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <summary>
	/// البحث في المنتجات (SmartSearch)
	/// </summary>
	public async Task<PagedProductResult?> SearchAsync(
		ProductSearchRequest? request = null,
		CancellationToken cancellationToken = default)
	{
		request ??= new ProductSearchRequest();
		return await _httpClient.PostAsync<ProductSearchRequest, PagedProductResult>(
			ServiceName,
			$"{BasePath}/search",
			request,
			cancellationToken);
	}

	/// <summary>
	/// الحصول على جميع المنتجات (يستخدم Search مع طلب فارغ)
	/// </summary>
	public async Task<List<ProductDto>?> GetAllAsync(CancellationToken cancellationToken = default)
	{
		var result = await SearchAsync(new ProductSearchRequest { PageSize = 100 }, cancellationToken);
		return result?.Items;
	}

	/// <summary>
	/// الحصول على منتج محدد
	/// </summary>
	public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<ProductDto>(
			ServiceName,
			$"{BasePath}/{id}",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على منتج محدد (string id للتوافق)
	/// </summary>
	public async Task<ProductDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<ProductDto>(
			ServiceName,
			$"{BasePath}/{id}",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على المنتجات المميزة
	/// </summary>
	public async Task<List<ProductDto>?> GetFeaturedAsync(
		int limit = 10,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<List<ProductDto>>(
			ServiceName,
			$"{BasePath}/featured?limit={limit}",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على المنتجات الجديدة
	/// </summary>
	public async Task<List<ProductDto>?> GetNewAsync(
		int limit = 10,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<List<ProductDto>>(
			ServiceName,
			$"{BasePath}/new?limit={limit}",
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
			BasePath,
			request,
			cancellationToken);
	}

	/// <summary>
	/// تحديث منتج
	/// </summary>
	public async Task<ProductDto?> UpdateAsync(
		Guid id,
		UpdateProductRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PutAsync<UpdateProductRequest, ProductDto>(
			ServiceName,
			$"{BasePath}/{id}",
			request,
			cancellationToken);
	}

	/// <summary>
	/// تحديث منتج (string id للتوافق)
	/// </summary>
	public async Task<ProductDto?> UpdateAsync(
		string id,
		UpdateProductRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PutAsync<UpdateProductRequest, ProductDto>(
			ServiceName,
			$"{BasePath}/{id}",
			request,
			cancellationToken);
	}

	/// <summary>
	/// حذف منتج
	/// </summary>
	public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
	{
		await _httpClient.DeleteAsync(
			ServiceName,
			$"{BasePath}/{id}",
			cancellationToken);
	}

	/// <summary>
	/// حذف منتج (string id للتوافق)
	/// </summary>
	public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
	{
		await _httpClient.DeleteAsync(
			ServiceName,
			$"{BasePath}/{id}",
			cancellationToken);
	}
}

/// <summary>
/// نتيجة البحث المقسمة لصفحات
/// </summary>
public sealed class PagedProductResult
{
	public List<ProductDto> Items { get; set; } = new();
	public int TotalCount { get; set; }
	public int PageNumber { get; set; }
	public int PageSize { get; set; }
	public int TotalPages { get; set; }
}

/// <summary>
/// طلب البحث في المنتجات
/// </summary>
public sealed class ProductSearchRequest
{
	public string? SearchTerm { get; set; }
	public List<FilterItem>? Filters { get; set; }
	public int PageNumber { get; set; } = 1;
	public int PageSize { get; set; } = 20;
	public string? OrderBy { get; set; }
	public bool Ascending { get; set; } = true;
	public List<string>? IncludeProperties { get; set; }
	public bool IncludeDeleted { get; set; }
}

/// <summary>
/// عنصر فلترة
/// </summary>
public sealed class FilterItem
{
	public string PropertyName { get; set; } = string.Empty;
	public object? Value { get; set; }
	public object? SecondValue { get; set; }
	public int Operator { get; set; }
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

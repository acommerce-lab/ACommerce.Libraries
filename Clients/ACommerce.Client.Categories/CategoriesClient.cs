using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Categories;

/// <summary>
/// Client لإدارة التصنيفات (Categories)
/// </summary>
public sealed class CategoriesClient
{
	private readonly IApiClient _httpClient;
	private const string ServiceName = "Marketplace";
	private const string BasePath = "/api/catalog/product-categories";

	public CategoriesClient(IApiClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <summary>
	/// البحث في التصنيفات (SmartSearch)
	/// </summary>
	public async Task<PagedCategoryResult?> SearchAsync(
		CategorySearchRequest? request = null,
		CancellationToken cancellationToken = default)
	{
		request ??= new CategorySearchRequest();
		return await _httpClient.PostAsync<CategorySearchRequest, PagedCategoryResult>(
			ServiceName,
			$"{BasePath}/search",
			request,
			cancellationToken);
	}

	/// <summary>
	/// الحصول على جميع التصنيفات
	/// </summary>
	public async Task<List<CategoryDto>?> GetAllAsync(
		CancellationToken cancellationToken = default)
	{
		var result = await SearchAsync(new CategorySearchRequest { PageSize = 100 }, cancellationToken);
		return result?.Items;
	}

	/// <summary>
	/// الحصول على تصنيف بالمعرف
	/// </summary>
	public async Task<CategoryDto?> GetByIdAsync(
		Guid categoryId,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<CategoryDto>(
			ServiceName,
			$"{BasePath}/{categoryId}",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على التصنيفات الجذرية (بدون أب)
	/// </summary>
	public async Task<List<CategoryDto>?> GetRootCategoriesAsync(
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<List<CategoryDto>>(
			ServiceName,
			$"{BasePath}/root",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على تصنيف بواسطة Slug
	/// </summary>
	public async Task<CategoryDto?> GetBySlugAsync(
		string slug,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<CategoryDto>(
			ServiceName,
			$"{BasePath}/by-slug/{slug}",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على التصنيفات الفرعية لتصنيف معين
	/// </summary>
	public async Task<List<CategoryDto>?> GetChildrenAsync(
		Guid categoryId,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<List<CategoryDto>>(
			ServiceName,
			$"{BasePath}/{categoryId}/children",
			cancellationToken);
	}
}

// ===== Models =====

/// <summary>
/// نتيجة البحث المقسمة لصفحات
/// </summary>
public sealed class PagedCategoryResult
{
	public List<CategoryDto> Items { get; set; } = new();
	public int TotalCount { get; set; }
	public int PageNumber { get; set; }
	public int PageSize { get; set; }
	public int TotalPages { get; set; }
}

/// <summary>
/// طلب البحث في التصنيفات
/// </summary>
public sealed class CategorySearchRequest
{
	public string? SearchTerm { get; set; }
	public List<CategoryFilterItem>? Filters { get; set; }
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
public sealed class CategoryFilterItem
{
	public string PropertyName { get; set; } = string.Empty;
	public object? Value { get; set; }
	public object? SecondValue { get; set; }
	public int Operator { get; set; }
}

public sealed class CategoryDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public string? Description { get; set; }
	public string? Image { get; set; }
	public string? Icon { get; set; }
	public int ProductsCount { get; set; }
	public int SubCategoriesCount { get; set; }
	public Guid? ParentCategoryId { get; set; }
	public string? ParentCategoryName { get; set; }
	public int SortOrder { get; set; }
	public bool IsActive { get; set; }
	public Dictionary<string, string> Metadata { get; set; } = new();
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}

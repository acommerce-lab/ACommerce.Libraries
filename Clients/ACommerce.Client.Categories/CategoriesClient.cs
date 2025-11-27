using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Categories;

/// <summary>
/// Client لإدارة التصنيفات (Categories)
/// </summary>
public sealed class CategoriesClient
{
	private readonly IApiClient _httpClient;
	private const string ServiceName = "Marketplace";

	public CategoriesClient(IApiClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <summary>
	/// الحصول على جميع التصنيفات
	/// </summary>
	public async Task<List<CategoryDto>?> GetAllAsync(
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<List<CategoryDto>>(
			ServiceName,
			"/api/categories",
			cancellationToken);
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
			$"/api/categories/{categoryId}",
			cancellationToken);
	}

	/// <summary>
	/// البحث في التصنيفات
	/// </summary>
	public async Task<List<CategoryDto>?> SearchAsync(
		string query,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<List<CategoryDto>>(
			ServiceName,
			$"/api/categories/search?q={query}",
			cancellationToken);
	}
}

// ===== Models =====

public sealed class CategoryDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	public string? ImageUrl { get; set; }
	public int ProductCount { get; set; }
	public Guid? ParentCategoryId { get; set; }
	public List<CategoryDto> SubCategories { get; set; } = new();
}

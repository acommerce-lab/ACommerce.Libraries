using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Categories;

/// <summary>
/// Client للحصول على خصائص الفئات الديناميكية
/// </summary>
public sealed class CategoryAttributesClient(IApiClient httpClient)
{
	private const string ServiceName = "Marketplace";
	private const string BasePath = "/api/CategoryAttributes";

	/// <summary>
	/// الحصول على الخصائص لفئة معينة
	/// </summary>
	public async Task<List<CategoryAttributeDto>?> GetAttributesForCategoryAsync(
		Guid categoryId,
		CancellationToken cancellationToken = default)
	{
		return await httpClient.GetAsync<List<CategoryAttributeDto>>(
			ServiceName,
			$"{BasePath}/category/{categoryId}",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على جميع الفئات المتاحة
	/// </summary>
	public async Task<List<CategoryInfoDto>?> GetAvailableCategoriesAsync(
		CancellationToken cancellationToken = default)
	{
		return await httpClient.GetAsync<List<CategoryInfoDto>>(
			ServiceName,
			$"{BasePath}/categories",
			cancellationToken);
	}

	/// <summary>
	/// الحصول على جميع ربطات الفئات بالخصائص
	/// </summary>
	public async Task<Dictionary<Guid, List<Guid>>?> GetAllMappingsAsync(
		CancellationToken cancellationToken = default)
	{
		return await httpClient.GetAsync<Dictionary<Guid, List<Guid>>>(
			ServiceName,
			$"{BasePath}/mappings",
			cancellationToken);
	}
}

/// <summary>
/// معلومات الفئة
/// </summary>
public sealed class CategoryInfoDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Slug { get; set; } = string.Empty;
	public string? Icon { get; set; }
	public string? Image { get; set; }
	public string? Description { get; set; }
}

/// <summary>
/// تعريف خاصية الفئة
/// </summary>
public sealed class CategoryAttributeDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Code { get; set; } = string.Empty;
	public string Type { get; set; } = "Text";
	public string? Description { get; set; }
	public bool IsRequired { get; set; }
	public bool IsFilterable { get; set; }
	public bool IsVisibleInList { get; set; } = true;
	public bool IsVisibleInDetail { get; set; } = true;
	public int SortOrder { get; set; }
	public string? ValidationRules { get; set; }
	public string? DefaultValue { get; set; }

	/// <summary>
	/// القيم الممكنة (للأنواع Select)
	/// </summary>
	public List<AttributeValueDto> Values { get; set; } = new();
}

/// <summary>
/// قيمة خاصية
/// </summary>
public sealed class AttributeValueDto
{
	public Guid Id { get; set; }
	public string Value { get; set; } = string.Empty;
	public string? DisplayName { get; set; }
	public string? Code { get; set; }
	public string? Description { get; set; }
	public string? ColorHex { get; set; }
	public string? ImageUrl { get; set; }
	public int SortOrder { get; set; }
	public bool IsActive { get; set; } = true;
}

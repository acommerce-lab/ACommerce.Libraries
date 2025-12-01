using Microsoft.AspNetCore.Mvc;
using Ashare.Api.Services;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.Catalog.Attributes.Entities;

namespace Ashare.Api.Controllers;

/// <summary>
/// نقاط نهاية خاصة بربط الخصائص بالفئات
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoryAttributesController : ControllerBase
{
	private readonly IRepositoryFactory _repositoryFactory;

	public CategoryAttributesController(IRepositoryFactory repositoryFactory)
	{
		_repositoryFactory = repositoryFactory;
	}

	/// <summary>
	/// الحصول على جميع ربطات الفئات بالخصائص
	/// </summary>
	[HttpGet("mappings")]
	public ActionResult<Dictionary<Guid, List<Guid>>> GetAllMappings()
	{
		return Ok(AshareSeedDataService.CategoryAttributeMappings);
	}

	/// <summary>
	/// الحصول على الخصائص لفئة معينة
	/// </summary>
	[HttpGet("category/{categoryId}")]
	public async Task<ActionResult> GetAttributesForCategory(Guid categoryId)
	{
		Console.WriteLine($"[CategoryAttributes] Getting attributes for category: {categoryId}");

		var attributeIds = AshareSeedDataService.GetAttributeIdsForCategory(categoryId);
		Console.WriteLine($"[CategoryAttributes] Found {attributeIds.Count} attribute IDs for category");

		if (attributeIds.Count == 0)
		{
			Console.WriteLine($"[CategoryAttributes] No attributes found for category {categoryId}");
			return NotFound(new { message = "الفئة غير موجودة أو لا تحتوي على خصائص" });
		}

		// جلب تفاصيل الخصائص من قاعدة البيانات مع تحميل القيم
		var repo = _repositoryFactory.CreateRepository<AttributeDefinition>();
		var allAttributes = await repo.GetAllWithPredicateAsync(
			predicate: null,
			includeDeleted: false,
			includeProperties: "Values");

		Console.WriteLine($"[CategoryAttributes] Total attributes in database: {allAttributes.Count}");

		var categoryAttributes = allAttributes
			.Where(a => attributeIds.Contains(a.Id))
			.OrderBy(a => attributeIds.IndexOf(a.Id))
			.Select(a => new
			{
				a.Id,
				a.Name,
				a.Code,
				Type = a.Type.ToString(),
				a.Description,
				a.IsRequired,
				a.IsFilterable,
				a.IsVisibleInList,
				a.IsVisibleInDetail,
				a.SortOrder,
				a.ValidationRules,
				a.DefaultValue,
				Values = a.Values.OrderBy(v => v.SortOrder).Select(v => new
				{
					v.Id,
					v.Value,
					v.DisplayName,
					v.Code,
					v.Description,
					v.ColorHex,
					v.ImageUrl,
					v.SortOrder,
					v.IsActive
				}).ToList()
			})
			.ToList();

		Console.WriteLine($"[CategoryAttributes] Returning {categoryAttributes.Count} attributes for category {categoryId}");

		return Ok(categoryAttributes);
	}

	/// <summary>
	/// الحصول على معرفات الفئات المتاحة
	/// </summary>
	[HttpGet("categories")]
	public ActionResult GetAvailableCategories()
	{
		var categories = new[]
		{
			new { Id = AshareSeedDataService.CategoryIds.Residential, Name = "سكني", Slug = "residential", Icon = "bi-house-door", Image = (string?)null, Description = "عرض سكن للإيجار أو المشاركة" },
			new { Id = AshareSeedDataService.CategoryIds.LookingForHousing, Name = "طلب سكن", Slug = "looking-for-housing", Icon = "bi-search", Image = (string?)null, Description = "أبحث عن سكن للإيجار أو المشاركة" },
			new { Id = AshareSeedDataService.CategoryIds.LookingForPartner, Name = "طلب شريك سكن", Slug = "looking-for-partner", Icon = "bi-people", Image = (string?)null, Description = "أبحث عن شريك سكن" },
			new { Id = AshareSeedDataService.CategoryIds.Administrative, Name = "مساحة إدارية", Slug = "administrative", Icon = "bi-building", Image = (string?)null, Description = "مكاتب ومساحات عمل مشتركة" },
			new { Id = AshareSeedDataService.CategoryIds.Commercial, Name = "مساحة تجارية", Slug = "commercial", Icon = "bi-shop", Image = (string?)null, Description = "محلات ومستودعات ومساحات تجارية" }
		};

		return Ok(categories);
	}

	/// <summary>
	/// Debug: الحصول على جميع الخصائص من قاعدة البيانات
	/// </summary>
	[HttpGet("debug/all-attributes")]
	public async Task<ActionResult> GetAllAttributesDebug()
	{
		var repo = _repositoryFactory.CreateRepository<AttributeDefinition>();
		var allAttributes = await repo.GetAllWithPredicateAsync(
			predicate: null,
			includeDeleted: false,
			includeProperties: "Values");

		return Ok(new
		{
			TotalCount = allAttributes.Count,
			Attributes = allAttributes.Select(a => new
			{
				a.Id,
				a.Name,
				a.Code,
				Type = a.Type.ToString(),
				ValuesCount = a.Values?.Count ?? 0
			}).ToList()
		});
	}
}

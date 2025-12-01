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
		var attributeIds = AshareSeedDataService.GetAttributeIdsForCategory(categoryId);

		if (attributeIds.Count == 0)
		{
			return NotFound(new { message = "الفئة غير موجودة أو لا تحتوي على خصائص" });
		}

		// جلب تفاصيل الخصائص من قاعدة البيانات مع تحميل القيم
		var repo = _repositoryFactory.CreateRepository<AttributeDefinition>();
		var allAttributes = await repo.GetAllWithPredicateAsync(
			predicate: null,
			includeDeleted: false,
			includeProperties: "Values");

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
			new { Id = AshareSeedDataService.CategoryIds.Residential, Name = "سكني", Slug = "residential" },
			new { Id = AshareSeedDataService.CategoryIds.LookingForHousing, Name = "طلب سكن", Slug = "looking-for-housing" },
			new { Id = AshareSeedDataService.CategoryIds.LookingForPartner, Name = "طلب شريك سكن", Slug = "looking-for-partner" },
			new { Id = AshareSeedDataService.CategoryIds.Administrative, Name = "مساحة إدارية", Slug = "administrative" },
			new { Id = AshareSeedDataService.CategoryIds.Commercial, Name = "مساحة تجارية", Slug = "commercial" }
		};

		return Ok(categories);
	}
}

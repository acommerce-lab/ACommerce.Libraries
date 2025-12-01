using Microsoft.AspNetCore.Mvc;
using Ashare.Api.Services;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.Catalog.Attributes.Entities;
using ACommerce.Catalog.Products.Entities;

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
	/// الحصول على جميع ربطات الفئات بالخصائص من قاعدة البيانات
	/// </summary>
	[HttpGet("mappings")]
	public async Task<ActionResult<Dictionary<Guid, List<Guid>>>> GetAllMappings()
	{
		var mappingRepo = _repositoryFactory.CreateRepository<CategoryAttributeMapping>();
		var allMappings = await mappingRepo.GetAllWithPredicateAsync(
			predicate: m => m.IsActive,
			includeDeleted: false);

		var result = allMappings
			.GroupBy(m => m.CategoryId)
			.ToDictionary(
				g => g.Key,
				g => g.OrderBy(m => m.SortOrder).Select(m => m.AttributeDefinitionId).ToList()
			);

		return Ok(result);
	}

	/// <summary>
	/// الحصول على الخصائص لفئة معينة من قاعدة البيانات
	/// </summary>
	[HttpGet("category/{categoryId}")]
	public async Task<ActionResult> GetAttributesForCategory(Guid categoryId)
	{
		Console.WriteLine($"[CategoryAttributes] Getting attributes for category: {categoryId}");

		// جلب الربطات من قاعدة البيانات مع تحميل الخصائص والقيم
		var mappingRepo = _repositoryFactory.CreateRepository<CategoryAttributeMapping>();
		var mappings = await mappingRepo.GetAllWithPredicateAsync(
			predicate: m => m.CategoryId == categoryId && m.IsActive,
			includeDeleted: false,
			includeProperties: "AttributeDefinition,AttributeDefinition.Values");

		Console.WriteLine($"[CategoryAttributes] Found {mappings.Count} mappings for category");

		if (mappings.Count == 0)
		{
			Console.WriteLine($"[CategoryAttributes] No attributes found for category {categoryId}");
			return NotFound(new { message = "الفئة غير موجودة أو لا تحتوي على خصائص" });
		}

		var categoryAttributes = mappings
			.OrderBy(m => m.SortOrder)
			.Select(m => new
			{
				m.AttributeDefinition.Id,
				m.AttributeDefinition.Name,
				m.AttributeDefinition.Code,
				Type = m.AttributeDefinition.Type.ToString(),
				m.AttributeDefinition.Description,
				// استخدام القيمة المتجاوزة إن وجدت
				IsRequired = m.IsRequiredOverride ?? m.AttributeDefinition.IsRequired,
				m.AttributeDefinition.IsFilterable,
				m.AttributeDefinition.IsVisibleInList,
				m.AttributeDefinition.IsVisibleInDetail,
				SortOrder = m.SortOrder,
				m.AttributeDefinition.ValidationRules,
				m.AttributeDefinition.DefaultValue,
				Values = m.AttributeDefinition.Values
					.Where(v => !v.IsDeleted && v.IsActive)
					.OrderBy(v => v.SortOrder)
					.Select(v => new
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
	/// الحصول على معرفات الفئات المتاحة من قاعدة البيانات
	/// </summary>
	[HttpGet("categories")]
	public async Task<ActionResult> GetAvailableCategories()
	{
		var categoryRepo = _repositoryFactory.CreateRepository<ProductCategory>();
		var categories = await categoryRepo.GetAllWithPredicateAsync(
			predicate: c => c.IsActive,
			includeDeleted: false);

		var result = categories
			.OrderBy(c => c.SortOrder)
			.Select(c => new
			{
				c.Id,
				c.Name,
				c.Slug,
				c.Icon,
				c.Image,
				c.Description
			})
			.ToList();

		return Ok(result);
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

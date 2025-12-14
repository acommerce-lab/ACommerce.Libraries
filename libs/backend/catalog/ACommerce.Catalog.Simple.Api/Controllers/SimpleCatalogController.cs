using ACommerce.Catalog.Attributes.Entities;
using ACommerce.Catalog.Products.Entities;
using ACommerce.Catalog.Simple.Api.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Repositories;

namespace ACommerce.Catalog.Simple.Api.Controllers;

/// <summary>
/// ????? ????? ?????? ???????? - ?????? ????????? ???????
/// </summary>
[ApiController]
[Route("api/simple-catalog")]
[Produces("application/json")]
public class SimpleCatalogController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly IBaseAsyncRepository<ProductCategory> _categoryRepository;
	private readonly IBaseAsyncRepository<AttributeDefinition> _attributeRepository;
	private readonly IBaseAsyncRepository<AttributeValue> _attributeValueRepository;
	private readonly ILogger<SimpleCatalogController> _logger;

	public SimpleCatalogController(
		IMediator mediator,
		IBaseAsyncRepository<ProductCategory> categoryRepository,
		IBaseAsyncRepository<AttributeDefinition> attributeRepository,
		IBaseAsyncRepository<AttributeValue> attributeValueRepository,
		ILogger<SimpleCatalogController> logger)
	{
		_mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
		_categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
		_attributeRepository = attributeRepository ?? throw new ArgumentNullException(nameof(attributeRepository));
		_attributeValueRepository = attributeValueRepository ?? throw new ArgumentNullException(nameof(attributeValueRepository));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	// ====================================================================================
	// ??????? (Categories)
	// ====================================================================================

	/// <summary>
	/// ?????? ??? ???? ??????? ?? ???????
	/// GET /api/simple-catalog/categories
	/// </summary>
	[HttpGet("categories")]
	[ProducesResponseType(typeof(List<SimpleCategoryDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<SimpleCategoryDto>>> GetCategories()
	{
		try
		{
			_logger.LogDebug("Getting all categories with attributes");

			var categories = await _categoryRepository.ListAllAsync();

			var result = new List<SimpleCategoryDto>();

			foreach (var category in categories)
			{
				// ?????? ??? ??????? ???????? ???? ?????
				var attributes = await GetCategoryAttributesAsync(category.Id);

				result.Add(new SimpleCategoryDto
				{
					Id = category.Id,
					Name = category.Name,
					Description = category.Description,
					Image = category.Image,
					Attributes = attributes,
					CreatedAt = category.CreatedAt
				});
			}

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting categories");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ??? ???? ?? ??????
	/// GET /api/simple-catalog/categories/{id}
	/// </summary>
	[HttpGet("categories/{id}")]
	[ProducesResponseType(typeof(SimpleCategoryDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<SimpleCategoryDto>> GetCategory(Guid id)
	{
		try
		{
			_logger.LogDebug("Getting category {CategoryId} with attributes", id);

			var category = await _categoryRepository.GetByIdAsync(id);
			if (category == null)
			{
				return NotFound(new { message = "Category not found" });
			}

			var attributes = await GetCategoryAttributesAsync(id);

			var result = new SimpleCategoryDto
			{
				Id = category.Id,
				Name = category.Name,
				Description = category.Description,
				Image = category.Image,
				Attributes = attributes,
				CreatedAt = category.CreatedAt
			};

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting category {CategoryId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ??? ???? ?? ??????
	/// POST /api/simple-catalog/categories
	/// </summary>
	[HttpPost("categories")]
	[ProducesResponseType(typeof(SimpleCategoryDto), 201)]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<SimpleCategoryDto>> CreateCategory(
		[FromBody] CreateSimpleCategoryRequest request)
	{
		try
		{
			_logger.LogDebug("Creating category {Name}", request.Name);

			// ????? ?????
			var category = new ProductCategory
			{
				Name = request.Name,
				Slug = GenerateSlug(request.Name),
				Description = request.Description,
				Image = request.Image
			};

			var createdCategory = await _categoryRepository.AddAsync(category);

			// ????? ???????
			var attributes = new List<SimpleAttributeDto>();
			if (request.Attributes != null)
			{
				foreach (var attrRequest in request.Attributes)
				{
					var attributeDto = await CreateAttributeForCategoryAsync(
						createdCategory.Id,
						attrRequest);
					attributes.Add(attributeDto);
				}
			}

			var result = new SimpleCategoryDto
			{
				Id = createdCategory.Id,
				Name = createdCategory.Name,
				Description = createdCategory.Description,
				Image = createdCategory.Image,
				Attributes = attributes,
				CreatedAt = createdCategory.CreatedAt
			};

			return CreatedAtAction(
				nameof(GetCategory),
				new { id = result.Id },
				result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating category");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ????? ????? ????
	/// POST /api/simple-catalog/categories/{categoryId}/attributes
	/// </summary>
	[HttpPost("categories/{categoryId}/attributes")]
	[ProducesResponseType(typeof(SimpleAttributeDto), 201)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<SimpleAttributeDto>> AddAttribute(
		Guid categoryId,
		[FromBody] CreateSimpleAttributeRequest request)
	{
		try
		{
			_logger.LogDebug("Adding attribute {Name} to category {CategoryId}", request.Name, categoryId);

			var category = await _categoryRepository.GetByIdAsync(categoryId);
			if (category == null)
			{
				return NotFound(new { message = "Category not found" });
			}

			var attributeDto = await CreateAttributeForCategoryAsync(categoryId, request);

			return CreatedAtAction(
				nameof(GetCategory),
				new { id = categoryId },
				attributeDto);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding attribute to category {CategoryId}", categoryId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ???? ????? ??????
	/// POST /api/simple-catalog/attributes/{attributeId}/values
	/// </summary>
	[HttpPost("attributes/{attributeId}/values")]
	[ProducesResponseType(typeof(SimpleAttributeValueDto), 201)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<SimpleAttributeValueDto>> AddAttributeValue(
		Guid attributeId,
		[FromBody] AddAttributeValueRequest request)
	{
		try
		{
			_logger.LogDebug("Adding value {Value} to attribute {AttributeId}", request.Value, attributeId);

			var attribute = await _attributeRepository.GetByIdAsync(attributeId);
			if (attribute == null)
			{
				return NotFound(new { message = "Attribute not found" });
			}

			var value = new AttributeValue
			{
				AttributeDefinitionId = attributeId,
				Value = request.Value,
				ColorHex = request.ColorHex,
				ImageUrl = request.ImageUrl,
				Code = GenerateSlug(request.Value)
			};

			var createdValue = await _attributeValueRepository.AddAsync(value);

			var result = new SimpleAttributeValueDto
			{
				Id = createdValue.Id,
				Value = createdValue.Value,
				ColorHex = createdValue.ColorHex,
				ImageUrl = createdValue.ImageUrl
			};

			return CreatedAtAction(
				nameof(GetCategory),
				new { id = attribute.Id },
				result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding value to attribute {AttributeId}", attributeId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	// ====================================================================================
	// Helper Methods
	// ====================================================================================

	private async Task<List<SimpleAttributeDto>> GetCategoryAttributesAsync(Guid categoryId)
	{
		var attributes = await _attributeRepository.GetAllWithPredicateAsync(
			a => a.Metadata.ContainsKey("CategoryId") && a.Metadata["CategoryId"] == categoryId.ToString(),
			includeDeleted: false);

		var result = new List<SimpleAttributeDto>();

		foreach (var attr in attributes)
		{
			var values = await _attributeValueRepository.GetAllWithPredicateAsync(
				v => v.AttributeDefinitionId == attr.Id,
				includeDeleted: false);

			result.Add(new SimpleAttributeDto
			{
				Id = attr.Id,
				Name = attr.Name,
				Values = values.Select(v => new SimpleAttributeValueDto
				{
					Id = v.Id,
					Value = v.Value,
					ColorHex = v.ColorHex,
					ImageUrl = v.ImageUrl
				}).ToList()
			});
		}

		return result;
	}

	private async Task<SimpleAttributeDto> CreateAttributeForCategoryAsync(
		Guid categoryId,
		CreateSimpleAttributeRequest request)
	{
		var attribute = new AttributeDefinition
		{
			Name = request.Name,
			Code = GenerateSlug(request.Name),
			Type = Attributes.Enums.AttributeType.SingleSelect,
			Metadata = new Dictionary<string, string>
			{
				{ "CategoryId", categoryId.ToString() }
			}
		};

		var createdAttribute = await _attributeRepository.AddAsync(attribute);

		var values = new List<SimpleAttributeValueDto>();
		if (request.Values != null)
		{
			foreach (var valueText in request.Values)
			{
				var value = new AttributeValue
				{
					AttributeDefinitionId = createdAttribute.Id,
					Value = valueText,
					Code = GenerateSlug(valueText)
				};

				var createdValue = await _attributeValueRepository.AddAsync(value);

				values.Add(new SimpleAttributeValueDto
				{
					Id = createdValue.Id,
					Value = createdValue.Value
				});
			}
		}

		return new SimpleAttributeDto
		{
			Id = createdAttribute.Id,
			Name = createdAttribute.Name,
			Values = values
		};
	}

	private static string GenerateSlug(string text)
	{
		return text.ToLowerInvariant()
			.Replace(" ", "-")
			.Replace("?", "a")
			.Replace("?", "b")
			.Replace("?", "t")
			.Replace("?", "th")
			.Replace("?", "j")
			.Replace("?", "h")
			.Replace("?", "kh")
			.Replace("?", "d")
			.Replace("?", "dh")
			.Replace("?", "r")
			.Replace("?", "z")
			.Replace("?", "s")
			.Replace("?", "sh")
			.Replace("?", "s")
			.Replace("?", "d")
			.Replace("?", "t")
			.Replace("?", "z")
			.Replace("?", "a")
			.Replace("?", "gh")
			.Replace("?", "f")
			.Replace("?", "q")
			.Replace("?", "k")
			.Replace("?", "l")
			.Replace("?", "m")
			.Replace("?", "n")
			.Replace("?", "h")
			.Replace("?", "w")
			.Replace("?", "y")
			.Trim('-');
	}
}

// DTOs ???????
public class AddAttributeValueRequest
{
	public required string Value { get; set; }
	public string? ColorHex { get; set; }
	public string? ImageUrl { get; set; }
}


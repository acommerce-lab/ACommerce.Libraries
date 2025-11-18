using ACommerce.Catalog.Products.DTOs.Product;
using ACommerce.Catalog.Products.DTOs.ProductCategory;
using ACommerce.Catalog.Products.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.Catalog.Products.Api.Controllers;

/// <summary>
/// ????? ???? ???????? (Product Categories)
/// </summary>
[ApiController]
[Route("api/catalog/product-categories")]
[Produces("application/json")]
public class ProductCategoriesController : BaseCrudController<
	ProductCategory,
	CreateProductCategoryDto,
	UpdateProductCategoryDto,
	ProductCategoryResponseDto,
	PartialUpdateProductCategoryDto>
{
	public ProductCategoriesController(
		IMediator mediator,
		ILogger<ProductCategoriesController> logger)
		: base(mediator, logger)
	{
	}

	/// <summary>
	/// ????? ?? ??? ???? Slug
	/// GET /api/catalog/product-categories/by-slug/{slug}
	/// </summary>
	[HttpGet("by-slug/{slug}")]
	[ProducesResponseType(typeof(ProductCategoryResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<ProductCategoryResponseDto>> GetBySlug(string slug)
	{
		try
		{
			_logger.LogDebug("Getting product category by slug {Slug}", slug);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(ProductCategory.Slug),
						Value = slug,
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 1,
				PageNumber = 1
			};

			var query = new SmartSearchQuery<ProductCategory, ProductCategoryResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				_logger.LogWarning("Product category with slug {Slug} not found", slug);
				return NotFound(new { message = "Category not found" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting product category by slug {Slug}", slug);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ?????? ??????? (Root Categories)
	/// GET /api/catalog/product-categories/root
	/// </summary>
	[HttpGet("root")]
	[ProducesResponseType(typeof(List<ProductCategoryResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<ProductCategoryResponseDto>>> GetRoot()
	{
		try
		{
			_logger.LogDebug("Getting root product categories");

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(ProductCategory.ParentCategoryId),
						Value = null,
						Operator = FilterOperator.IsNull
					},
					new()
					{
						PropertyName = nameof(ProductCategory.IsActive),
						Value = true,
						Operator = FilterOperator.Equals
					}
				},
				OrderBy = nameof(ProductCategory.SortOrder),
				Ascending = true,
				PageSize = 100
			};

			var query = new SmartSearchQuery<ProductCategory, ProductCategoryResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting root product categories");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ?????? ???????
	/// GET /api/catalog/product-categories/{id}/children
	/// </summary>
	[HttpGet("{id}/children")]
	[ProducesResponseType(typeof(List<ProductCategoryResponseDto>), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<ProductCategoryResponseDto>>> GetChildren(Guid id)
	{
		try
		{
			_logger.LogDebug("Getting children for product category {CategoryId}", id);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(ProductCategory.ParentCategoryId),
						Value = id,
						Operator = FilterOperator.Equals
					}
				},
				OrderBy = nameof(ProductCategory.SortOrder),
				Ascending = true,
				PageSize = 100
			};

			var query = new SmartSearchQuery<ProductCategory, ProductCategoryResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting children for product category {CategoryId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ???????? ?? ??? ?????
	/// GET /api/catalog/product-categories/{id}/products
	/// </summary>
	[HttpGet("{id}/products")]
	[ProducesResponseType(typeof(PagedResult<ProductResponseDto>), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<ProductResponseDto>>> GetProducts(
		Guid id,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 20)
	{
		try
		{
			_logger.LogDebug("Getting products for category {CategoryId}", id);

			// TODO: ??????? ProductCategoryMapping ?????? ??? ????????

			return Ok(new PagedResult<ProductResponseDto>());
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting products for category {CategoryId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

// DTOs ??????
public class UpdateProductCategoryDto
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public string? Image { get; set; }
	public string? Icon { get; set; }
	public Guid? ParentCategoryId { get; set; }
	public int? SortOrder { get; set; }
	public bool? IsActive { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

public class PartialUpdateProductCategoryDto
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public string? Image { get; set; }
	public string? Icon { get; set; }
	public Guid? ParentCategoryId { get; set; }
	public int? SortOrder { get; set; }
	public bool? IsActive { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}


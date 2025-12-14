using ACommerce.Catalog.Products.DTOs.Product;
using ACommerce.Catalog.Products.Entities;
using ACommerce.Catalog.Products.Enums;
using ACommerce.Catalog.Products.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.Catalog.Products.Api.Controllers;

/// <summary>
/// ????? ???????? (Products)
/// </summary>
[ApiController]
[Route("api/catalog/products")]
[Produces("application/json")]
public class ProductsController : BaseCrudController<
	Product,
	CreateProductDto,
	UpdateProductDto,
	ProductResponseDto,
	PartialUpdateProductDto>
{
	private readonly IProductService _productService;

	public ProductsController(
		IMediator mediator,
		IProductService productService,
		ILogger<ProductsController> logger)
		: base(mediator, logger)
	{
		_productService = productService ?? throw new ArgumentNullException(nameof(productService));
	}

	// ====================================================================================
	// ??? Endpoints ???????? ?????? ?? BaseCrudController
	// ====================================================================================

	// ====================================================================================
	// Custom Endpoints
	// ====================================================================================

	/// <summary>
	/// ?????? ??? ?????? ?????? ???????
	/// GET /api/catalog/products/{id}/detail
	/// </summary>
	[HttpGet("{id}/detail")]
	[ProducesResponseType(typeof(ProductDetailResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<ProductDetailResponseDto>> GetDetail(
		Guid id,
		[FromQuery] string? currencyCode = null,
		[FromQuery] string? market = null)
	{
		try
		{
			_logger.LogDebug("Getting product detail for {ProductId}", id);

			var detail = await _productService.GetProductDetailAsync(id, currencyCode, market);

			if (detail == null)
			{
				_logger.LogWarning("Product {ProductId} not found", id);
				return NotFound(new { message = "Product not found" });
			}

			return Ok(detail);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting product detail for {ProductId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ?? ???? ???? SKU
	/// GET /api/catalog/products/by-sku/{sku}
	/// </summary>
	[HttpGet("by-sku/{sku}")]
	[ProducesResponseType(typeof(ProductResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<ProductResponseDto>> GetBySku(string sku)
	{
		try
		{
			_logger.LogDebug("Getting product by SKU {Sku}", sku);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(Product.Sku),
						Value = sku,
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 1,
				PageNumber = 1
			};

			var query = new SmartSearchQuery<Product, ProductResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				_logger.LogWarning("Product with SKU {Sku} not found", sku);
				return NotFound(new { message = "Product not found" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting product by SKU {Sku}", sku);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ?? ???? ?????????
	/// GET /api/catalog/products/by-barcode/{barcode}
	/// </summary>
	[HttpGet("by-barcode/{barcode}")]
	[ProducesResponseType(typeof(ProductResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<ProductResponseDto>> GetByBarcode(string barcode)
	{
		try
		{
			_logger.LogDebug("Getting product by barcode {Barcode}", barcode);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(Product.Barcode),
						Value = barcode,
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 1,
				PageNumber = 1
			};

			var query = new SmartSearchQuery<Product, ProductResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				_logger.LogWarning("Product with barcode {Barcode} not found", barcode);
				return NotFound(new { message = "Product not found" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting product by barcode {Barcode}", barcode);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ???????? ???????
	/// GET /api/catalog/products/featured
	/// </summary>
	[HttpGet("featured")]
	[ProducesResponseType(typeof(List<ProductResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<ProductResponseDto>>> GetFeatured(
		[FromQuery] int limit = 10)
	{
		try
		{
			_logger.LogDebug("Getting featured products");

			var products = await _productService.GetFeaturedProductsAsync(limit);

			return Ok(products);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting featured products");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ???????? ???????
	/// GET /api/catalog/products/new
	/// </summary>
	[HttpGet("new")]
	[ProducesResponseType(typeof(List<ProductResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<ProductResponseDto>>> GetNew(
		[FromQuery] int limit = 10)
	{
		try
		{
			_logger.LogDebug("Getting new products");

			var products = await _productService.GetNewProductsAsync(limit);

			return Ok(products);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting new products");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ??????? ?? ????????
	/// POST /api/catalog/products/advanced-search
	/// </summary>
	[HttpPost("advanced-search")]
	[ProducesResponseType(typeof(List<ProductResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<ProductResponseDto>>> AdvancedSearch(
		[FromBody] ProductSearchRequest request)
	{
		try
		{
			_logger.LogDebug("Advanced product search with query {Query}", request.Query);

			var products = await _productService.SearchProductsAsync(request);

			return Ok(products);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in advanced product search");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ???????? ??? ??????
	/// GET /api/catalog/products/by-status/{status}
	/// </summary>
	[HttpGet("by-status/{status}")]
	[ProducesResponseType(typeof(PagedResult<ProductResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<ProductResponseDto>>> GetByStatus(
		ProductStatus status,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 20)
	{
		try
		{
			_logger.LogDebug("Getting products by status {Status}", status);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(Product.Status),
						Value = status,
						Operator = FilterOperator.Equals
					}
				},
				PageNumber = pageNumber,
				PageSize = pageSize,
				OrderBy = nameof(Product.CreatedAt),
				Ascending = false
			};

			var query = new SmartSearchQuery<Product, ProductResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting products by status {Status}", status);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ????????? (Variants)
	/// GET /api/catalog/products/{id}/variants
	/// </summary>
	[HttpGet("{id}/variants")]
	[ProducesResponseType(typeof(List<ProductResponseDto>), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<ProductResponseDto>>> GetVariants(Guid id)
	{
		try
		{
			_logger.LogDebug("Getting variants for product {ProductId}", id);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(Product.ParentProductId),
						Value = id,
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 100
			};

			var query = new SmartSearchQuery<Product, ProductResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting variants for product {ProductId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ??? ??????
	/// POST /api/catalog/products/{id}/categories
	/// </summary>
	[HttpPost("{id}/categories")]
	[ProducesResponseType(204)]
	[ProducesResponseType(400)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> AddCategory(
		Guid id,
		[FromBody] AddCategoryRequest request)
	{
		try
		{
			_logger.LogDebug("Adding category {CategoryId} to product {ProductId}", request.CategoryId, id);

			await _productService.AddCategoryAsync(id, request.CategoryId, request.IsPrimary);

			return NoContent();
		}
		catch (ArgumentException ex)
		{
			_logger.LogWarning(ex, "Invalid request to add category to product {ProductId}", id);
			return BadRequest(new { message = ex.Message });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding category to product {ProductId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ??? ?? ??????
	/// DELETE /api/catalog/products/{id}/categories/{categoryId}
	/// </summary>
	[HttpDelete("{id}/categories/{categoryId}")]
	[ProducesResponseType(204)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> RemoveCategory(Guid id, Guid categoryId)
	{
		try
		{
			_logger.LogDebug("Removing category {CategoryId} from product {ProductId}", categoryId, id);

			await _productService.RemoveCategoryAsync(id, categoryId);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error removing category from product {ProductId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ????? ?????? ??????
	/// POST /api/catalog/products/{id}/brands
	/// </summary>
	[HttpPost("{id}/brands")]
	[ProducesResponseType(204)]
	[ProducesResponseType(400)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> AddBrand(
		Guid id,
		[FromBody] AddBrandRequest request)
	{
		try
		{
			_logger.LogDebug("Adding brand {BrandId} to product {ProductId}", request.BrandId, id);

			await _productService.AddBrandAsync(id, request.BrandId);

			return NoContent();
		}
		catch (ArgumentException ex)
		{
			_logger.LogWarning(ex, "Invalid request to add brand to product {ProductId}", id);
			return BadRequest(new { message = ex.Message });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding brand to product {ProductId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ????? ??????
	/// POST /api/catalog/products/{id}/attributes
	/// </summary>
	[HttpPost("{id}/attributes")]
	[ProducesResponseType(204)]
	[ProducesResponseType(400)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> AddAttribute(
		Guid id,
		[FromBody] AddAttributeRequest request)
	{
		try
		{
			_logger.LogDebug("Adding attribute to product {ProductId}", id);

			await _productService.AddAttributeAsync(
				id,
				request.AttributeDefinitionId,
				request.AttributeValueId,
				request.CustomValue,
				request.IsVariant);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding attribute to product {ProductId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ???? ?????
	/// POST /api/catalog/products/{id}/related
	/// </summary>
	[HttpPost("{id}/related")]
	[ProducesResponseType(204)]
	[ProducesResponseType(400)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> AddRelatedProduct(
		Guid id,
		[FromBody] AddRelatedProductRequest request)
	{
		try
		{
			_logger.LogDebug("Adding related product {RelatedProductId} to product {ProductId}", request.RelatedProductId, id);

			await _productService.AddRelatedProductAsync(id, request.RelatedProductId, request.RelationType);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding related product to product {ProductId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

// DTOs ???????
public class AddCategoryRequest
{
	public Guid CategoryId { get; set; }
	public bool IsPrimary { get; set; }
}

public class AddBrandRequest
{
	public Guid BrandId { get; set; }
}

public class AddAttributeRequest
{
	public Guid AttributeDefinitionId { get; set; }
	public Guid? AttributeValueId { get; set; }
	public string? CustomValue { get; set; }
	public bool IsVariant { get; set; }
}

public class AddRelatedProductRequest
{
	public Guid RelatedProductId { get; set; }
	public string RelationType { get; set; } = "related";
}


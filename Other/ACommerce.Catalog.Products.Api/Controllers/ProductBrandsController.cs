using ACommerce.Catalog.Products.DTOs.Product;
using ACommerce.Catalog.Products.DTOs.ProductBrand;
using ACommerce.Catalog.Products.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.Catalog.Products.Api.Controllers;

/// <summary>
/// ????? ???????? ???????? (Product Brands)
/// </summary>
[ApiController]
[Route("api/catalog/product-brands")]
[Produces("application/json")]
public class ProductBrandsController : BaseCrudController<
	ProductBrand,
	CreateProductBrandDto,
	UpdateProductBrandDto,
	ProductBrandResponseDto,
	PartialUpdateProductBrandDto>
{
	public ProductBrandsController(
		IMediator mediator,
		ILogger<ProductBrandsController> logger)
		: base(mediator, logger)
	{
	}

	/// <summary>
	/// ????? ?? ????? ?????? ???? Slug
	/// GET /api/catalog/product-brands/by-slug/{slug}
	/// </summary>
	[HttpGet("by-slug/{slug}")]
	[ProducesResponseType(typeof(ProductBrandResponseDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<ProductBrandResponseDto>> GetBySlug(string slug)
	{
		try
		{
			_logger.LogDebug("Getting product brand by slug {Slug}", slug);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(ProductBrand.Slug),
						Value = slug,
						Operator = FilterOperator.Equals
					}
				},
				PageSize = 1,
				PageNumber = 1
			};

			var query = new SmartSearchQuery<ProductBrand, ProductBrandResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			if (result.Items.Count == 0)
			{
				_logger.LogWarning("Product brand with slug {Slug} not found", slug);
				return NotFound(new { message = "Brand not found" });
			}

			return Ok(result.Items[0]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting product brand by slug {Slug}", slug);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ???????? ???????? ??? ??????
	/// GET /api/catalog/product-brands/by-country/{country}
	/// </summary>
	[HttpGet("by-country/{country}")]
	[ProducesResponseType(typeof(List<ProductBrandResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<ProductBrandResponseDto>>> GetByCountry(string country)
	{
		try
		{
			_logger.LogDebug("Getting product brands by country {Country}", country);

			var searchRequest = new SmartSearchRequest
			{
				Filters = new List<FilterItem>
				{
					new()
					{
						PropertyName = nameof(ProductBrand.Country),
						Value = country,
						Operator = FilterOperator.Equals
					},
					new()
					{
						PropertyName = nameof(ProductBrand.IsActive),
						Value = true,
						Operator = FilterOperator.Equals
					}
				},
				OrderBy = nameof(ProductBrand.Name),
				Ascending = true,
				PageSize = 100
			};

			var query = new SmartSearchQuery<ProductBrand, ProductBrandResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting product brands by country {Country}", country);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ???????? ?????? ??????
	/// GET /api/catalog/product-brands/{id}/products
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
			_logger.LogDebug("Getting products for brand {BrandId}", id);

			// TODO: ??????? ProductBrandMapping ?????? ??? ????????

			return Ok(new PagedResult<ProductResponseDto>());
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting products for brand {BrandId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

// DTOs ??????
public class UpdateProductBrandDto
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public string? Logo { get; set; }
	public string? Website { get; set; }
	public string? Country { get; set; }
	public int? SortOrder { get; set; }
	public bool? IsActive { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}

public class PartialUpdateProductBrandDto
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public string? Logo { get; set; }
	public string? Website { get; set; }
	public string? Country { get; set; }
	public int? SortOrder { get; set; }
	public bool? IsActive { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}


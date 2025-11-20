using ACommerce.Catalog.Products.DTOs.Product;
using ACommerce.Catalog.Products.DTOs.ProductPrice;
using ACommerce.Catalog.Products.Entities;
using ACommerce.Catalog.Products.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.AspNetCore.Controllers;

namespace ACommerce.Catalog.Products.Api.Controllers;

/// <summary>
/// ????? ????? ???????? (Product Prices)
/// </summary>
[ApiController]
[Route("api/catalog/product-prices")]
[Produces("application/json")]
public class ProductPricesController : BaseCrudController<
	ProductPrice,
	CreateProductPriceDto,
	UpdateProductPriceDto,
	ProductPriceDto,
	PartialUpdateProductPriceDto>
{
	private readonly IProductService _productService;

	public ProductPricesController(
		IMediator mediator,
		IProductService productService,
		ILogger<ProductPricesController> logger)
		: base(mediator, logger)
	{
		_productService = productService ?? throw new ArgumentNullException(nameof(productService));
	}

	/// <summary>
	/// ?????? ??? ????? ??????
	/// GET /api/catalog/product-prices/effective
	/// </summary>
	[HttpGet("effective")]
	[ProducesResponseType(typeof(ProductPriceDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<ProductPriceDto>> GetEffective(
		[FromQuery] Guid productId,
		[FromQuery] string currencyCode,
		[FromQuery] string? market = null,
		[FromQuery] string? customerSegment = null)
	{
		try
		{
			_logger.LogDebug("Getting effective price for product {ProductId}", productId);

			var price = await _productService.GetEffectivePriceAsync(
				productId,
				currencyCode,
				market,
				customerSegment);

			if (price == null)
			{
				return NotFound(new { message = "No price found for specified criteria" });
			}

			return Ok(price);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting effective price for product {ProductId}", productId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

// DTOs ??????
public class UpdateProductPriceDto
{
	public decimal? BasePrice { get; set; }
	public decimal? SalePrice { get; set; }
	public decimal? DiscountPercentage { get; set; }
	public DateTime? SaleStartDate { get; set; }
	public DateTime? SaleEndDate { get; set; }
	public int? MinQuantity { get; set; }
	public int? MaxQuantity { get; set; }
	public bool? IsActive { get; set; }
}

public class PartialUpdateProductPriceDto
{
	public decimal? BasePrice { get; set; }
	public decimal? SalePrice { get; set; }
	public decimal? DiscountPercentage { get; set; }
	public DateTime? SaleStartDate { get; set; }
	public DateTime? SaleEndDate { get; set; }
	public int? MinQuantity { get; set; }
	public int? MaxQuantity { get; set; }
	public bool? IsActive { get; set; }
}


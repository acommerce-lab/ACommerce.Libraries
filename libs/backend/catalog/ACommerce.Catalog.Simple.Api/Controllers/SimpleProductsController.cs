using ACommerce.Catalog.Currencies.Entities;
using ACommerce.Catalog.Products.Entities;
using ACommerce.Catalog.Products.Enums;
using ACommerce.Catalog.Simple.Api.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Repositories;

namespace ACommerce.Catalog.Simple.Api.Controllers;

/// <summary>
/// ????? ????? ?????? ????????
/// </summary>
[ApiController]
[Route("api/simple-catalog/products")]
[Produces("application/json")]
public class SimpleProductsController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly IBaseAsyncRepository<Product> _productRepository;
	private readonly IBaseAsyncRepository<ProductCategory> _categoryRepository;
	private readonly IBaseAsyncRepository<ProductPrice> _priceRepository;
	private readonly IBaseAsyncRepository<ProductInventory> _inventoryRepository;
	private readonly IBaseAsyncRepository<Currency> _currencyRepository;
	private readonly ILogger<SimpleProductsController> _logger;

	public SimpleProductsController(
		IMediator mediator,
		IBaseAsyncRepository<Product> productRepository,
		IBaseAsyncRepository<ProductCategory> categoryRepository,
		IBaseAsyncRepository<ProductPrice> priceRepository,
		IBaseAsyncRepository<ProductInventory> inventoryRepository,
		IBaseAsyncRepository<Currency> currencyRepository,
		ILogger<SimpleProductsController> logger)
	{
		_mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
		_productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
		_categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
		_priceRepository = priceRepository ?? throw new ArgumentNullException(nameof(priceRepository));
		_inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
		_currencyRepository = currencyRepository ?? throw new ArgumentNullException(nameof(currencyRepository));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// ?????? ??? ???? ????????
	/// GET /api/simple-catalog/products
	/// </summary>
	[HttpGet]
	[ProducesResponseType(typeof(List<SimpleProductDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<List<SimpleProductDto>>> GetProducts(
		[FromQuery] Guid? categoryId = null,
		[FromQuery] bool? inStockOnly = null,
		[FromQuery] bool? featuredOnly = null)
	{
		try
		{
			_logger.LogDebug("Getting products");

			var products = await _productRepository.ListAllAsync();

			// ????? ???????
			if (categoryId.HasValue)
			{
				products = [.. products.Where(p =>
					p.Categories.Any(c => c.Id == categoryId.Value))];
			}

			var result = new List<SimpleProductDto>();

			foreach (var product in products)
			{
				var dto = await MapToSimpleProductDto(product);

				// ????? ??? ???????
				if (inStockOnly == true && !dto.InStock)
					continue;

				// ????? ??? ??????
				if (featuredOnly == true && !dto.IsFeatured)
					continue;

				result.Add(dto);
			}

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting products");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ??? ???? ????
	/// GET /api/simple-catalog/products/{id}
	/// </summary>
	[HttpGet("{id}")]
	[ProducesResponseType(typeof(SimpleProductDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<SimpleProductDto>> GetProduct(Guid id)
	{
		try
		{
			_logger.LogDebug("Getting product {ProductId}", id);

			var product = await _productRepository.GetByIdAsync(id);
			if (product == null)
			{
				return NotFound(new { message = "Product not found" });
			}

			var result = await MapToSimpleProductDto(product);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting product {ProductId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ???? ????
	/// POST /api/simple-catalog/products
	/// </summary>
	[HttpPost]
	[ProducesResponseType(typeof(SimpleProductDto), 201)]
	[ProducesResponseType(400)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<SimpleProductDto>> CreateProduct(
		[FromBody] CreateSimpleProductRequest request)
	{
		try
		{
			_logger.LogDebug("Creating product {Name}", request.Name);

			// ?????? ?? ?????
			var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
			if (category == null)
			{
				return BadRequest(new { message = "Category not found" });
			}

			// ????? ??????
			var product = new Product
			{
				Name = request.Name,
				Sku = request.Sku,
				ShortDescription = request.Description,
				Type = ProductType.Simple,
				Status = ProductStatus.Active,
				FeaturedImage = request.Image,
				Images = request.Images ?? new List<string>(),
				IsFeatured = request.IsFeatured,
				IsNew = request.IsNew
			};

			var createdProduct = await _productRepository.AddAsync(product);

			// ??? ?????? ??????
			var categoryMapping = new ProductCategoryMapping
			{
				ProductId = createdProduct.Id,
				CategoryId = request.CategoryId,
				IsPrimary = true
			};
			// TODO: Add category mapping

			// ?????? ??? ?????? ??????????
			var defaultCurrency = await GetDefaultCurrencyAsync();

			// ????? ?????
			var price = new ProductPrice
			{
				ProductId = createdProduct.Id,
				CurrencyId = defaultCurrency.Id,
				BasePrice = request.Price,
				SalePrice = request.SalePrice
			};
			await _priceRepository.AddAsync(price);

			// ????? ???????
			var inventory = new ProductInventory
			{
				ProductId = createdProduct.Id,
				QuantityInStock = request.Stock,
				TrackInventory = true,
				Status = request.Stock > 0 ? StockStatus.InStock : StockStatus.OutOfStock
			};
			await _inventoryRepository.AddAsync(inventory);

			// TODO: ????? ??????? ?? request.Attributes

			var result = await MapToSimpleProductDto(createdProduct);

			return CreatedAtAction(
				nameof(GetProduct),
				new { id = result.Id },
				result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating product");
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ????
	/// PUT /api/simple-catalog/products/{id}
	/// </summary>
	[HttpPut("{id}")]
	[ProducesResponseType(typeof(SimpleProductDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<SimpleProductDto>> UpdateProduct(
		Guid id,
		[FromBody] UpdateSimpleProductRequest request)
	{
		try
		{
			_logger.LogDebug("Updating product {ProductId}", id);

			var product = await _productRepository.GetByIdAsync(id);
			if (product == null)
			{
				return NotFound(new { message = "Product not found" });
			}

			// ????? ??????
			if (request.Name != null) product.Name = request.Name;
			if (request.Description != null) product.ShortDescription = request.Description;
			if (request.Image != null) product.FeaturedImage = request.Image;
			if (request.Images != null) product.Images = request.Images;
			if (request.IsFeatured.HasValue) product.IsFeatured = request.IsFeatured.Value;
			if (request.IsNew.HasValue) product.IsNew = request.IsNew.Value;

			await _productRepository.UpdateAsync(product);

			// ????? ?????
			if (request.Price.HasValue || request.SalePrice.HasValue)
			{
				var prices = await _priceRepository.GetAllWithPredicateAsync(
					p => p.ProductId == id,
					includeDeleted: false);

				var price = prices.FirstOrDefault();
				if (price != null)
				{
					if (request.Price.HasValue) price.BasePrice = request.Price.Value;
					if (request.SalePrice.HasValue) price.SalePrice = request.SalePrice.Value;
					await _priceRepository.UpdateAsync(price);
				}
			}

			// ????? ???????
			if (request.Stock.HasValue)
			{
				var inventories = await _inventoryRepository.GetAllWithPredicateAsync(
					i => i.ProductId == id,
					includeDeleted: false);

				var inventory = inventories.FirstOrDefault();
				if (inventory != null)
				{
					inventory.QuantityInStock = request.Stock.Value;
					inventory.Status = request.Stock.Value > 0 ? StockStatus.InStock : StockStatus.OutOfStock;
					await _inventoryRepository.UpdateAsync(inventory);
				}
			}

			var result = await MapToSimpleProductDto(product);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating product {ProductId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ??? ????
	/// DELETE /api/simple-catalog/products/{id}
	/// </summary>
	[HttpDelete("{id}")]
	[ProducesResponseType(204)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> DeleteProduct(Guid id)
	{
		try
		{
			_logger.LogDebug("Deleting product {ProductId}", id);

			var product = await _productRepository.GetByIdAsync(id);
			if (product == null)
			{
				return NotFound(new { message = "Product not found" });
			}

			await _productRepository.DeleteAsync(id);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting product {ProductId}", id);
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

	private async Task<SimpleProductDto> MapToSimpleProductDto(Product product)
	{
		// ?????? ??? ?????
		var prices = await _priceRepository.GetAllWithPredicateAsync(
			p => p.ProductId == product.Id,
			includeDeleted: false);
		var price = prices.FirstOrDefault();

		// ?????? ??? ???????
		var inventories = await _inventoryRepository.GetAllWithPredicateAsync(
			i => i.ProductId == product.Id,
			includeDeleted: false);
		var inventory = inventories.FirstOrDefault();

		// ?????? ??? ?????
		var categoryMapping = product.Categories.FirstOrDefault();
		var category = categoryMapping != null
			? await _categoryRepository.GetByIdAsync(categoryMapping.Id)
			: null;

		// TODO: ?????? ??? ???????

		return new SimpleProductDto
		{
			Id = product.Id,
			Name = product.Name,
			Sku = product.Sku,
			Description = product.ShortDescription,
			Price = price?.BasePrice ?? 0,
			SalePrice = price?.SalePrice,
			Image = product.FeaturedImage,
			Images = product.Images,
			CategoryId = category?.Id ?? Guid.Empty,
			CategoryName = category?.Name ?? string.Empty,
			Attributes = new Dictionary<string, string>(),
			Stock = (int)(inventory?.QuantityInStock ?? 0),
			InStock = inventory?.AvailableQuantity > 0,
			IsFeatured = product.IsFeatured,
			IsNew = product.IsNew,
			CreatedAt = product.CreatedAt
		};
	}

	private async Task<Currency> GetDefaultCurrencyAsync()
	{
		var currencies = await _currencyRepository.GetAllWithPredicateAsync(
			c => c.IsBaseCurrency,
			includeDeleted: false);

		var defaultCurrency = currencies.FirstOrDefault();

		if (defaultCurrency == null)
		{
			// ????? ???? ???????? ??? ?? ????
			defaultCurrency = new Currency
			{
				Name = "Saudi Riyal",
				Code = "SAR",
				Symbol = "?",
				IsBaseCurrency = true,
				SymbolBeforeAmount = false
			};
			defaultCurrency = await _currencyRepository.AddAsync(defaultCurrency);
		}

		return defaultCurrency;
	}
}


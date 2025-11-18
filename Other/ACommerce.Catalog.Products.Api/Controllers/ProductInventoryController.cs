using ACommerce.Catalog.Products.DTOs.Product;
using ACommerce.Catalog.Products.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ACommerce.Catalog.Products.Api.Controllers;

/// <summary>
/// ????? ????? ???????? (Product Inventory)
/// </summary>
[ApiController]
[Route("api/catalog/product-inventory")]
[Produces("application/json")]
public class ProductInventoryController : ControllerBase
{
	private readonly IInventoryService _inventoryService;
	private readonly IMediator _mediator;
	private readonly ILogger<ProductInventoryController> _logger;

	public ProductInventoryController(
		IInventoryService inventoryService,
		IMediator mediator,
		ILogger<ProductInventoryController> logger)
	{
		_inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
		_mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// ?????? ??? ???????
	/// GET /api/catalog/product-inventory/{productId}
	/// </summary>
	[HttpGet("{productId}")]
	[ProducesResponseType(typeof(ProductInventoryDto), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<ProductInventoryDto>> GetInventory(Guid productId)
	{
		try
		{
			_logger.LogDebug("Getting inventory for product {ProductId}", productId);

			var inventory = await _inventoryService.GetInventoryAsync(productId);

			if (inventory == null)
			{
				return NotFound(new { message = "Inventory not found" });
			}

			var dto = new ProductInventoryDto
			{
				QuantityInStock = inventory.QuantityInStock,
				AvailableQuantity = inventory.AvailableQuantity,
				Status = inventory.Status,
				TrackInventory = inventory.TrackInventory,
				AllowBackorder = inventory.AllowBackorder
			};

			return Ok(dto);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting inventory for product {ProductId}", productId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ???? ???????
	/// POST /api/catalog/product-inventory/{productId}/add-stock
	/// </summary>
	[HttpPost("{productId}/add-stock")]
	[ProducesResponseType(204)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> AddStock(
		Guid productId,
		[FromBody] StockAdjustmentRequest request)
	{
		try
		{
			_logger.LogDebug("Adding {Quantity} stock to product {ProductId}", request.Quantity, productId);

			await _inventoryService.AddStockAsync(productId, request.Quantity, request.Warehouse);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding stock to product {ProductId}", productId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ??? ???? ?? ???????
	/// POST /api/catalog/product-inventory/{productId}/deduct-stock
	/// </summary>
	[HttpPost("{productId}/deduct-stock")]
	[ProducesResponseType(204)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> DeductStock(
		Guid productId,
		[FromBody] StockAdjustmentRequest request)
	{
		try
		{
			_logger.LogDebug("Deducting {Quantity} stock from product {ProductId}", request.Quantity, productId);

			await _inventoryService.DeductStockAsync(productId, request.Quantity, request.Warehouse);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deducting stock from product {ProductId}", productId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ??? ????
	/// POST /api/catalog/product-inventory/{productId}/reserve
	/// </summary>
	[HttpPost("{productId}/reserve")]
	[ProducesResponseType(204)]
	[ProducesResponseType(400)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> Reserve(
		Guid productId,
		[FromBody] ReserveStockRequest request)
	{
		try
		{
			_logger.LogDebug("Reserving {Quantity} stock for product {ProductId}", request.Quantity, productId);

			await _inventoryService.ReserveStockAsync(productId, request.Quantity);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error reserving stock for product {ProductId}", productId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ??? ????
	/// POST /api/catalog/product-inventory/{productId}/release
	/// </summary>
	[HttpPost("{productId}/release")]
	[ProducesResponseType(204)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> Release(
		Guid productId,
		[FromBody] ReleaseStockRequest request)
	{
		try
		{
			_logger.LogDebug("Releasing {Quantity} stock for product {ProductId}", request.Quantity, productId);

			await _inventoryService.ReleaseReservationAsync(productId, request.Quantity);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error releasing stock for product {ProductId}", productId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ?? ??????
	/// GET /api/catalog/product-inventory/{productId}/check-availability
	/// </summary>
	[HttpGet("{productId}/check-availability")]
	[ProducesResponseType(typeof(bool), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<bool>> CheckAvailability(
		Guid productId,
		[FromQuery] decimal quantity)
	{
		try
		{
			_logger.LogDebug("Checking availability of {Quantity} for product {ProductId}", quantity, productId);

			var isAvailable = await _inventoryService.IsAvailableAsync(productId, quantity);

			return Ok(isAvailable);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error checking availability for product {ProductId}", productId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

// DTOs ???????
public class StockAdjustmentRequest
{
	public decimal Quantity { get; set; }
	public string? Warehouse { get; set; }
}

public class ReserveStockRequest
{
	public decimal Quantity { get; set; }
}

public class ReleaseStockRequest
{
	public decimal Quantity { get; set; }
}


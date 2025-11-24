using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.Orders.Entities;
using ACommerce.Orders.DTOs;
using ACommerce.SharedKernel.AspNetCore.Controllers;

namespace ACommerce.Orders.Api.Controllers;

/// <summary>
/// متحكم الطلبات
/// </summary>
public class OrdersController : BaseCrudController<Order, CreateOrderDto, CreateOrderDto, OrderResponseDto, CreateOrderDto>
{
	public OrdersController(
		IMediator mediator,
		ILogger<OrdersController> logger)
		: base(mediator, logger)
	{
	}

	/// <summary>
	/// الحصول على طلبات العميل
	/// </summary>
	[HttpGet("customer/{customerId}")]
	public async Task<ActionResult> GetCustomerOrders(string customerId)
	{
		try
		{
			var searchRequest = new SharedKernel.Abstractions.Queries.SmartSearchRequest
			{
				PageSize = 50,
				PageNumber = 1,
				Filters = new List<SharedKernel.Abstractions.Queries.FilterItem>
				{
					new() { PropertyName = "CustomerId", Value = customerId, Operator = SharedKernel.Abstractions.Queries.FilterOperator.Equals }
				},
				SortBy = "CreatedAt",
				SortDescending = true
			};

			var query = new SharedKernel.CQRS.Queries.SmartSearchQuery<Order, OrderResponseDto> { Request = searchRequest };
			var result = await _mediator.Send(query);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting orders for customer {CustomerId}", customerId);
			return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
		}
	}

	/// <summary>
	/// الحصول على طلبات البائع
	/// </summary>
	[HttpGet("vendor/{vendorId}")]
	public async Task<ActionResult> GetVendorOrders(Guid vendorId)
	{
		try
		{
			var searchRequest = new SharedKernel.Abstractions.Queries.SmartSearchRequest
			{
				PageSize = 50,
				PageNumber = 1,
				Filters = new List<SharedKernel.Abstractions.Queries.FilterItem>
				{
					new() { PropertyName = "VendorId", Value = vendorId.ToString(), Operator = SharedKernel.Abstractions.Queries.FilterOperator.Equals }
				},
				SortBy = "CreatedAt",
				SortDescending = true
			};

			var query = new SharedKernel.CQRS.Queries.SmartSearchQuery<Order, OrderResponseDto> { Request = searchRequest };
			var result = await _mediator.Send(query);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting orders for vendor {VendorId}", vendorId);
			return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
		}
	}

	/// <summary>
	/// تأكيد الطلب
	/// </summary>
	[HttpPost("{id}/confirm")]
	public async Task<IActionResult> ConfirmOrder(Guid id)
	{
		// سيتم التنفيذ لاحقاً
		return NoContent();
	}

	/// <summary>
	/// شحن الطلب
	/// </summary>
	[HttpPost("{id}/ship")]
	public async Task<IActionResult> ShipOrder(Guid id, [FromBody] string trackingNumber)
	{
		// سيتم التنفيذ لاحقاً
		return NoContent();
	}

	/// <summary>
	/// إلغاء الطلب
	/// </summary>
	[HttpPost("{id}/cancel")]
	public async Task<IActionResult> CancelOrder(Guid id, [FromBody] string? reason)
	{
		// سيتم التنفيذ لاحقاً
		return NoContent();
	}
}

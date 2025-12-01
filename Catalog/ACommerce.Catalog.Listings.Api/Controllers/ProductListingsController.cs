using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.Catalog.Listings.Entities;
using ACommerce.Catalog.Listings.DTOs;
using ACommerce.SharedKernel.AspNetCore.Controllers;

namespace ACommerce.Catalog.Listings.Api.Controllers;

/// <summary>
/// متحكم عروض المنتجات
/// </summary>
public class ProductListingsController(
    IMediator mediator,
    ILogger<ProductListingsController> logger) : BaseCrudController<ProductListing, CreateListingDto, CreateListingDto, ProductListingResponseDto, CreateListingDto>(mediator, logger)
{

    /// <summary>
    /// الحصول على جميع عروض منتج معين
    /// </summary>
    [HttpGet("by-product/{productId}")]
	public async Task<ActionResult> GetByProduct(Guid productId, [FromQuery] bool activeOnly = true)
	{
		try
		{
			var searchRequest = new SharedKernel.Abstractions.Queries.SmartSearchRequest
			{
				PageSize = 100,
				PageNumber = 1,
				Filters =
                [
                    new() { PropertyName = "ProductId", Value = productId.ToString(), Operator = SharedKernel.Abstractions.Queries.FilterOperator.Equals }
				]
			};

			if (activeOnly)
			{
				searchRequest.Filters.Add(new() { PropertyName = "IsActive", Value = "true", Operator = SharedKernel.Abstractions.Queries.FilterOperator.Equals });
			}

			var query = new SharedKernel.CQRS.Queries.SmartSearchQuery<ProductListing, ProductListingResponseDto> { Request = searchRequest };
			var result = await _mediator.Send(query);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting listings for product {ProductId}", productId);
			return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
		}
	}

	/// <summary>
	/// الحصول على جميع عروض بائع معين
	/// </summary>
	[HttpGet("by-vendor/{vendorId}")]
	public async Task<ActionResult> GetByVendor(Guid vendorId)
	{
		try
		{
			var searchRequest = new SharedKernel.Abstractions.Queries.SmartSearchRequest
			{
				PageSize = 100,
				PageNumber = 1,
				Filters = new List<SharedKernel.Abstractions.Queries.FilterItem>
				{
					new() { PropertyName = "VendorId", Value = vendorId.ToString(), Operator = SharedKernel.Abstractions.Queries.FilterOperator.Equals }
				}
			};

			var query = new SharedKernel.CQRS.Queries.SmartSearchQuery<ProductListing, ProductListingResponseDto> { Request = searchRequest };
			var result = await _mediator.Send(query);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting listings for vendor {VendorId}", vendorId);
			return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
		}
	}
}

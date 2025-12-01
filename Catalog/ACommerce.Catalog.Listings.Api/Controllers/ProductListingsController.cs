using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.Catalog.Listings.Entities;
using ACommerce.Catalog.Listings.Enums;
using ACommerce.Catalog.Listings.DTOs;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.Catalog.Listings.Api.Controllers;

/// <summary>
/// متحكم عروض المنتجات
/// </summary>
[ApiController]
[Route("api/listings")]
[Produces("application/json")]
public class ProductListingsController(
    IMediator mediator,
    ILogger<ProductListingsController> logger) : BaseCrudController<ProductListing, CreateProductListingDto, CreateProductListingDto, ProductListingResponseDto, CreateProductListingDto>(mediator, logger)
{
	/// <summary>
	/// الحصول على العروض المميزة
	/// </summary>
	[HttpGet("featured")]
	[ProducesResponseType(typeof(List<ProductListingResponseDto>), 200)]
	public async Task<ActionResult<List<ProductListingResponseDto>>> GetFeatured([FromQuery] int limit = 10)
	{
		try
		{
			var searchRequest = new SmartSearchRequest
			{
				PageSize = limit,
				PageNumber = 1,
				Filters = new List<FilterItem>
				{
					new() { PropertyName = "IsActive", Value = true, Operator = FilterOperator.Equals },
					new() { PropertyName = "Status", Value = ListingStatus.Active, Operator = FilterOperator.Equals }
				},
				OrderBy = "ViewCount",
				Ascending = false
			};

			var query = new SmartSearchQuery<ProductListing, ProductListingResponseDto> { Request = searchRequest };
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting featured listings");
			return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
		}
	}

	/// <summary>
	/// الحصول على العروض الجديدة
	/// </summary>
	[HttpGet("new")]
	[ProducesResponseType(typeof(List<ProductListingResponseDto>), 200)]
	public async Task<ActionResult<List<ProductListingResponseDto>>> GetNew([FromQuery] int limit = 10)
	{
		try
		{
			var searchRequest = new SmartSearchRequest
			{
				PageSize = limit,
				PageNumber = 1,
				Filters = new List<FilterItem>
				{
					new() { PropertyName = "IsActive", Value = true, Operator = FilterOperator.Equals },
					new() { PropertyName = "Status", Value = ListingStatus.Active, Operator = FilterOperator.Equals }
				},
				OrderBy = "CreatedAt",
				Ascending = false
			};

			var query = new SmartSearchQuery<ProductListing, ProductListingResponseDto> { Request = searchRequest };
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting new listings");
			return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
		}
	}

	/// <summary>
	/// الحصول على العروض حسب الفئة
	/// </summary>
	[HttpGet("category/{categoryId}")]
	[ProducesResponseType(typeof(List<ProductListingResponseDto>), 200)]
	public async Task<ActionResult<List<ProductListingResponseDto>>> GetByCategory(Guid categoryId, [FromQuery] int limit = 20)
	{
		try
		{
			// TODO: Add CategoryId filter when ProductListing supports it
			var searchRequest = new SmartSearchRequest
			{
				PageSize = limit,
				PageNumber = 1,
				Filters = new List<FilterItem>
				{
					new() { PropertyName = "IsActive", Value = true, Operator = FilterOperator.Equals },
					new() { PropertyName = "Status", Value = ListingStatus.Active, Operator = FilterOperator.Equals }
				},
				OrderBy = "CreatedAt",
				Ascending = false
			};

			var query = new SmartSearchQuery<ProductListing, ProductListingResponseDto> { Request = searchRequest };
			var result = await _mediator.Send(query);

			return Ok(result.Items);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting listings for category {CategoryId}", categoryId);
			return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
		}
	}

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

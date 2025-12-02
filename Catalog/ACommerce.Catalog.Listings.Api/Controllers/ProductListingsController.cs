using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.Catalog.Listings.Entities;
using ACommerce.Catalog.Listings.Enums;
using ACommerce.Catalog.Listings.DTOs;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.CQRS.Queries;
using ACommerce.SharedKernel.CQRS.Commands;

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
	/// الحصول على جميع العروض النشطة
	/// </summary>
	[HttpGet]
	[ProducesResponseType(typeof(List<ProductListingResponseDto>), 200)]
	public async Task<ActionResult<List<ProductListingResponseDto>>> GetAll([FromQuery] int limit = 50)
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
			_logger.LogError(ex, "Error getting all listings");
			return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
		}
	}

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
			var searchRequest = new SmartSearchRequest
			{
				PageSize = limit,
				PageNumber = 1,
				Filters = new List<FilterItem>
				{
					new() { PropertyName = "IsActive", Value = true, Operator = FilterOperator.Equals },
					new() { PropertyName = "Status", Value = ListingStatus.Active, Operator = FilterOperator.Equals },
					new() { PropertyName = "CategoryId", Value = categoryId, Operator = FilterOperator.Equals }
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

	/// <summary>
	/// الحصول على عروض المستخدم الحالي
	/// </summary>
	[HttpGet("my-listings")]
	[Authorize]
	[ProducesResponseType(typeof(List<ProductListingResponseDto>), 200)]
	public async Task<ActionResult<List<ProductListingResponseDto>>> GetMyListings()
	{
		try
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var vendorId))
			{
				return Unauthorized(new { message = "User not authenticated" });
			}

			var searchRequest = new SmartSearchRequest
			{
				PageSize = 100,
				PageNumber = 1,
				Filters = new List<FilterItem>
				{
					new() { PropertyName = "VendorId", Value = vendorId, Operator = FilterOperator.Equals }
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
			_logger.LogError(ex, "Error getting my listings");
			return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
		}
	}

	/// <summary>
	/// إنشاء عرض جديد (يتطلب مصادقة)
	/// VendorId يُستخرج تلقائياً من المستخدم المصادق
	/// </summary>
	[HttpPost]
	[Authorize]
	[ProducesResponseType(typeof(ProductListing), 201)]
	[ProducesResponseType(401)]
	[ProducesResponseType(400)]
	public override async Task<ActionResult<ProductListing>> Create([FromBody] CreateProductListingDto dto)
	{
		try
		{
			// استخراج معرف المستخدم من التوكن
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var vendorId))
			{
				_logger.LogWarning("Unauthorized attempt to create listing - no valid user ID");
				return Unauthorized(new { message = "User not authenticated" });
			}

			// تعيين VendorId من المستخدم المصادق (تجاهل أي قيمة مرسلة من العميل)
			dto.VendorId = vendorId;

			_logger.LogInformation("Creating listing for vendor {VendorId}", vendorId);

			var command = new CreateCommand<ProductListing, CreateProductListingDto> { Data = dto };
			var result = await _mediator.Send(command);

			_logger.LogInformation("Created listing {ListingId} for vendor {VendorId}", result.Id, vendorId);

			return CreatedAtAction(
				actionName: nameof(GetById),
				routeValues: new { id = result.Id },
				value: result);
		}
		catch (FluentValidation.ValidationException vex)
		{
			_logger.LogWarning("Validation failed for listing: {Errors}", vex.Errors);
			return BadRequest(new
			{
				message = "Validation failed",
				errors = vex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating listing");
			return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
		}
	}

	/// <summary>
	/// تحديث عرض (يتطلب مصادقة ويجب أن يكون المالك)
	/// </summary>
	[HttpPut("{id}")]
	[Authorize]
	[ProducesResponseType(204)]
	[ProducesResponseType(401)]
	[ProducesResponseType(403)]
	[ProducesResponseType(404)]
	public override async Task<IActionResult> Update(Guid id, [FromBody] CreateProductListingDto dto)
	{
		try
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var vendorId))
			{
				return Unauthorized(new { message = "User not authenticated" });
			}

			// التحقق من ملكية العرض
			var existingQuery = new GetByIdQuery<ProductListing, ProductListingResponseDto> { Id = id };
			var existing = await _mediator.Send(existingQuery);

			if (existing == null)
			{
				return NotFound(new { message = "Listing not found" });
			}

			if (existing.VendorId != vendorId)
			{
				_logger.LogWarning("User {UserId} attempted to update listing {ListingId} owned by {OwnerId}",
					vendorId, id, existing.VendorId);
				return Forbid();
			}

			// الحفاظ على VendorId الأصلي
			dto.VendorId = vendorId;

			var command = new UpdateCommand<ProductListing, CreateProductListingDto> { Id = id, Data = dto };
			await _mediator.Send(command);

			_logger.LogInformation("Updated listing {ListingId} by vendor {VendorId}", id, vendorId);

			return NoContent();
		}
		catch (KeyNotFoundException)
		{
			return NotFound(new { message = "Listing not found" });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating listing {ListingId}", id);
			return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
		}
	}

	/// <summary>
	/// حذف عرض (يتطلب مصادقة ويجب أن يكون المالك)
	/// </summary>
	[HttpDelete("{id}")]
	[Authorize]
	[ProducesResponseType(204)]
	[ProducesResponseType(401)]
	[ProducesResponseType(403)]
	[ProducesResponseType(404)]
	public override async Task<IActionResult> Delete(Guid id)
	{
		try
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var vendorId))
			{
				return Unauthorized(new { message = "User not authenticated" });
			}

			// التحقق من ملكية العرض
			var existingQuery = new GetByIdQuery<ProductListing, ProductListingResponseDto> { Id = id };
			var existing = await _mediator.Send(existingQuery);

			if (existing == null)
			{
				return NotFound(new { message = "Listing not found" });
			}

			if (existing.VendorId != vendorId)
			{
				_logger.LogWarning("User {UserId} attempted to delete listing {ListingId} owned by {OwnerId}",
					vendorId, id, existing.VendorId);
				return Forbid();
			}

			var command = new DeleteCommand<ProductListing> { Id = id };
			await _mediator.Send(command);

			_logger.LogInformation("Deleted listing {ListingId} by vendor {VendorId}", id, vendorId);

			return NoContent();
		}
		catch (KeyNotFoundException)
		{
			return NotFound(new { message = "Listing not found" });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting listing {ListingId}", id);
			return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
		}
	}
}

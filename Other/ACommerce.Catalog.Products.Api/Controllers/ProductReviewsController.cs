using ACommerce.Catalog.Products.DTOs.ProductReview;
using ACommerce.Catalog.Products.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.CQRS.Commands;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.Catalog.Products.Api.Controllers;

/// <summary>
/// ????? ??????? ???????? (Product Reviews)
/// </summary>
[ApiController]
[Route("api/catalog/product-reviews")]
[Produces("application/json")]
public class ProductReviewsController : BaseCrudController<
	ProductReview,
	CreateProductReviewDto,
	UpdateProductReviewDto,
	ProductReviewResponseDto,
	PartialUpdateProductReviewDto>
{
	public ProductReviewsController(
		IMediator mediator,
		ILogger<ProductReviewsController> logger)
		: base(mediator, logger)
	{
	}

	/// <summary>
	/// ?????? ??? ??????? ????
	/// GET /api/catalog/product-reviews/by-product/{productId}
	/// </summary>
	[HttpGet("by-product/{productId}")]
	[ProducesResponseType(typeof(PagedResult<ProductReviewResponseDto>), 200)]
	[ProducesResponseType(500)]
	public async Task<ActionResult<PagedResult<ProductReviewResponseDto>>> GetByProduct(
		Guid productId,
		[FromQuery] int pageNumber = 1,
		[FromQuery] int pageSize = 20,
		[FromQuery] bool approvedOnly = true)
	{
		try
		{
			_logger.LogDebug("Getting reviews for product {ProductId}", productId);

			var filters = new List<FilterItem>
			{
				new()
				{
					PropertyName = nameof(ProductReview.ProductId),
					Value = productId,
					Operator = FilterOperator.Equals
				}
			};

			if (approvedOnly)
			{
				filters.Add(new FilterItem
				{
					PropertyName = nameof(ProductReview.IsApproved),
					Value = true,
					Operator = FilterOperator.Equals
				});
			}

			var searchRequest = new SmartSearchRequest
			{
				Filters = filters,
				PageNumber = pageNumber,
				PageSize = pageSize,
				OrderBy = nameof(ProductReview.CreatedAt),
				Ascending = false
			};

			var query = new SmartSearchQuery<ProductReview, ProductReviewResponseDto>
			{
				Request = searchRequest
			};
			var result = await _mediator.Send(query);

			return Ok(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting reviews for product {ProductId}", productId);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ?????? ?????
	/// POST /api/catalog/product-reviews/{id}/approve
	/// </summary>
	[HttpPost("{id}/approve")]
	[ProducesResponseType(204)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> Approve(Guid id)
	{
		try
		{
			_logger.LogDebug("Approving review {ReviewId}", id);

			var updateDto = new PartialUpdateProductReviewDto
			{
				IsApproved = true
			};

			var command = new PartialUpdateCommand<ProductReview, PartialUpdateProductReviewDto>
			{
				Id = id,
				Data = updateDto
			};

			await _mediator.Send(command);

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error approving review {ReviewId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}

	/// <summary>
	/// ????? ????? ????
	/// POST /api/catalog/product-reviews/{id}/vote-helpful
	/// </summary>
	[HttpPost("{id}/vote-helpful")]
	[ProducesResponseType(204)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> VoteHelpful(Guid id)
	{
		try
		{
			_logger.LogDebug("Adding helpful vote to review {ReviewId}", id);

			// TODO: ????? ???? ????? ????? ????

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding helpful vote to review {ReviewId}", id);
			return StatusCode(500, new
			{
				message = "An error occurred while processing your request",
				detail = ex.Message
			});
		}
	}
}

// DTOs ??????
public class UpdateProductReviewDto
{
	public string? Title { get; set; }
	public string? Comment { get; set; }
	public int? Rating { get; set; }
	public bool? IsApproved { get; set; }
}

public class PartialUpdateProductReviewDto
{
	public string? Title { get; set; }
	public string? Comment { get; set; }
	public int? Rating { get; set; }
	public bool? IsApproved { get; set; }
}


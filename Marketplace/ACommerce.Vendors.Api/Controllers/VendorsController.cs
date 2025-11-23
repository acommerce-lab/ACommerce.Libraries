using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.Vendors.Entities;
using ACommerce.Vendors.DTOs;
using ACommerce.SharedKernel.AspNetCore.Controllers;

namespace ACommerce.Vendors.Api.Controllers;

/// <summary>
/// متحكم البائعين
/// </summary>
public class VendorsController : BaseCrudController<Vendor, CreateVendorDto, CreateVendorDto, VendorResponseDto, CreateVendorDto>
{
	public VendorsController(
		IMediator mediator,
		ILogger<VendorsController> logger)
		: base(mediator, logger)
	{
	}

	/// <summary>
	/// الحصول على بائع بواسطة الـ Slug
	/// </summary>
	[HttpGet("by-slug/{slug}")]
	public async Task<ActionResult<VendorResponseDto>> GetBySlug(string slug)
	{
		try
		{
			var searchRequest = new SharedKernel.Abstractions.Queries.SmartSearchRequest
			{
				PageSize = 1,
				PageNumber = 1,
				Filters = new List<SharedKernel.Abstractions.Queries.FilterItem>
				{
					new() { PropertyName = "StoreSlug", Value = slug, Operator = SharedKernel.Abstractions.Queries.FilterOperator.Equals }
				}
			};

			var query = new SharedKernel.CQRS.Queries.SmartSearchQuery<Vendor, VendorResponseDto> { Request = searchRequest };
			var result = await _mediator.Send(query);

			if (result.Data.Count == 0)
				return NotFound(new { message = "Vendor not found" });

			return Ok(result.Data.First());
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting vendor by slug {Slug}", slug);
			return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
		}
	}

	/// <summary>
	/// تفعيل البائع
	/// </summary>
	[HttpPost("{id}/activate")]
	public async Task<IActionResult> Activate(Guid id)
	{
		// سيتم تنفيذ منطق التفعيل لاحقاً
		return NoContent();
	}

	/// <summary>
	/// تعليق البائع
	/// </summary>
	[HttpPost("{id}/suspend")]
	public async Task<IActionResult> Suspend(Guid id)
	{
		// سيتم تنفيذ منطق التعليق لاحقاً
		return NoContent();
	}
}

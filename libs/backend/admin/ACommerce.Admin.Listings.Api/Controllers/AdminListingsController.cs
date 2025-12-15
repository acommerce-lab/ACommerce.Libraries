using ACommerce.Admin.Listings.DTOs;
using ACommerce.Admin.Listings.Queries;
using ACommerce.Catalog.Listings.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ACommerce.Admin.Listings.Api.Controllers;

[ApiController]
[Route("api/admin/listings")]
[Authorize(Roles = "Admin")]
public class AdminListingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminListingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<AdminListingListDto>> GetListings(
        [FromQuery] ListingStatus? status,
        [FromQuery] bool? isActive,
        [FromQuery] Guid? vendorId,
        [FromQuery] Guid? categoryId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? searchTerm,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string orderBy = "CreatedAt",
        [FromQuery] bool ascending = false)
    {
        var filter = new AdminListingFilterDto
        {
            Status = status,
            IsActive = isActive,
            VendorId = vendorId,
            CategoryId = categoryId,
            StartDate = startDate,
            EndDate = endDate,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            SearchTerm = searchTerm,
            Page = page,
            PageSize = Math.Min(pageSize, 100),
            OrderBy = orderBy,
            Ascending = ascending
        };

        var result = await _mediator.Send(new GetAdminListingsQuery(filter));
        return Ok(result);
    }
}

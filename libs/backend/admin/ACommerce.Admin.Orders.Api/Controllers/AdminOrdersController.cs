using ACommerce.Admin.Orders.DTOs;
using ACommerce.Admin.Orders.Queries;
using ACommerce.Orders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ACommerce.Admin.Orders.Api.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<AdminOrderListDto>> GetOrders(
        [FromQuery] OrderStatus? status,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? vendorId,
        [FromQuery] Guid? customerId,
        [FromQuery] decimal? minTotal,
        [FromQuery] decimal? maxTotal,
        [FromQuery] string? searchTerm,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string orderBy = "CreatedAt",
        [FromQuery] bool ascending = false)
    {
        var filter = new AdminOrderFilterDto
        {
            Status = status,
            StartDate = startDate,
            EndDate = endDate,
            VendorId = vendorId,
            CustomerId = customerId,
            MinTotal = minTotal,
            MaxTotal = maxTotal,
            SearchTerm = searchTerm,
            Page = page,
            PageSize = Math.Min(pageSize, 100),
            OrderBy = orderBy,
            Ascending = ascending
        };

        var result = await _mediator.Send(new GetAdminOrdersQuery(filter));
        return Ok(result);
    }
}

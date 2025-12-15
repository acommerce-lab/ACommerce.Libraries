using ACommerce.Admin.Dashboard.DTOs;
using ACommerce.Admin.Dashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ACommerce.Admin.Dashboard.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats()
    {
        var result = await _mediator.Send(new GetDashboardStatsQuery());
        return Ok(result);
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<List<RevenueByPeriodDto>>> GetRevenue([FromQuery] int days = 30)
    {
        var result = await _mediator.Send(new GetRevenueByPeriodQuery(days));
        return Ok(result);
    }

    [HttpGet("top-listings")]
    public async Task<ActionResult<List<TopListingDto>>> GetTopListings([FromQuery] int count = 10)
    {
        var result = await _mediator.Send(new GetTopListingsQuery(count));
        return Ok(result);
    }
}

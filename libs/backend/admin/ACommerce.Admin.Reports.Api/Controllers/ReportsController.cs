using ACommerce.Admin.Reports.DTOs;
using ACommerce.Admin.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ACommerce.Admin.Reports.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("sales")]
    public async Task<ActionResult<SalesReportDto>> GetSalesReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var result = await _mediator.Send(new GetSalesReportQuery(startDate, endDate));
        return Ok(result);
    }

    [HttpGet("users")]
    public async Task<ActionResult<UserActivityReportDto>> GetUserActivityReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var result = await _mediator.Send(new GetUserActivityReportQuery(startDate, endDate));
        return Ok(result);
    }

    [HttpGet("vendors")]
    public async Task<ActionResult<VendorReportDto>> GetVendorReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int topCount = 10)
    {
        var result = await _mediator.Send(new GetVendorReportQuery(startDate, endDate, topCount));
        return Ok(result);
    }
}

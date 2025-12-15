using ACommerce.Admin.AuditLog.Commands;
using ACommerce.Admin.AuditLog.DTOs;
using ACommerce.Admin.AuditLog.Entities;
using ACommerce.Admin.AuditLog.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ACommerce.Admin.AuditLog.Api.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditLogController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditLogController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<AuditLogListDto>> GetLogs(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? userId,
        [FromQuery] string? action,
        [FromQuery] string? entityType,
        [FromQuery] AuditLogLevel? level,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new AuditLogFilterDto
        {
            StartDate = startDate,
            EndDate = endDate,
            UserId = userId,
            Action = action,
            EntityType = entityType,
            Level = level,
            Page = page,
            PageSize = Math.Min(pageSize, 100)
        };

        var result = await _mediator.Send(new GetAuditLogsQuery(filter));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateLog([FromBody] CreateAuditLogDto dto)
    {
        var result = await _mediator.Send(new CreateAuditLogCommand(dto));
        return Ok(result);
    }
}

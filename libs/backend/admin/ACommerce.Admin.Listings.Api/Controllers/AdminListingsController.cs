using ACommerce.Admin.AuditLog.Commands;
using ACommerce.Admin.AuditLog.Entities;
using ACommerce.Admin.AuditLog.Services;
using ACommerce.Admin.Listings.Commands;
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

    // ════════════════════════════════════════════════════════════════
    // Mutations — each writes an audit log entry on success.
    // ════════════════════════════════════════════════════════════════

    /// <summary>الموافقة على عرض (Status → Active، IsActive → true).</summary>
    [HttpPost("{id:guid}/approve")]
    public Task<IActionResult> Approve(Guid id, [FromBody] ChangeListingStatusRequest? body = null)
        => ChangeStatus(id, ListingStatus.Active, body?.Reason, "Approve");

    /// <summary>رفض عرض (Status → Rejected، IsActive → false). السبب إلزامي.</summary>
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ChangeListingStatusRequest body)
    {
        if (string.IsNullOrWhiteSpace(body?.Reason))
            return BadRequest(new { message = "Rejection reason is required" });
        return await ChangeStatus(id, ListingStatus.Rejected, body.Reason, "Reject");
    }

    /// <summary>تعليق عرض (Status → Suspended، IsActive → false).</summary>
    [HttpPost("{id:guid}/suspend")]
    public Task<IActionResult> Suspend(Guid id, [FromBody] ChangeListingStatusRequest? body = null)
        => ChangeStatus(id, ListingStatus.Suspended, body?.Reason, "Suspend");

    /// <summary>تغيير حالة عرض إلى أي قيمة (للحالات المتقدّمة).</summary>
    [HttpPost("{id:guid}/status/{status}")]
    public Task<IActionResult> SetStatus(Guid id, ListingStatus status, [FromBody] ChangeListingStatusRequest? body = null)
        => ChangeStatus(id, status, body?.Reason, "ChangeStatus");

    /// <summary>تفعيل/تعطيل ظهور العرض دون تغيير حالة المراجعة.</summary>
    [HttpPatch("{id:guid}/active")]
    public async Task<IActionResult> SetActive(Guid id, [FromBody] SetListingActiveRequest body)
    {
        var result = await _mediator.Send(new SetListingActiveCommand(id, body.IsActive));
        if (!result.Success) return NotFound(new { message = result.Message });

        await WriteAudit(
            action: body.IsActive ? "Activate" : "Deactivate",
            entityId: id,
            details: $"'{result.Title}' active: {result.OldIsActive} → {result.NewIsActive}",
            newValues: $"{{\"isActive\":{result.NewIsActive.ToString().ToLower()}}}");

        return Ok(result);
    }

    /// <summary>حذف ناعم لعرض.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteListingCommand(id));
        if (!result.Success) return NotFound(new { message = result.Message });

        await WriteAudit(
            action: "Delete",
            entityId: id,
            details: $"Soft-deleted listing '{result.Title}'",
            level: AuditLogLevel.Warning);

        return Ok(result);
    }

    // ── helpers ───────────────────────────────────────────────────

    private async Task<IActionResult> ChangeStatus(Guid id, ListingStatus status, string? reason, string action)
    {
        var result = await _mediator.Send(new ChangeListingStatusCommand(id, status, reason));
        if (!result.Success) return NotFound(new { message = result.Message });

        await WriteAudit(
            action: action,
            entityId: id,
            details: $"'{result.Title}' status: {result.OldStatus} → {result.NewStatus}"
                     + (string.IsNullOrWhiteSpace(reason) ? "" : $" (reason: {reason})"),
            oldValues: $"{{\"status\":\"{result.OldStatus}\"}}",
            newValues: $"{{\"status\":\"{result.NewStatus}\"}}",
            level: status is ListingStatus.Rejected or ListingStatus.Suspended ? AuditLogLevel.Warning : AuditLogLevel.Info);

        return Ok(result);
    }

    private async Task WriteAudit(
        string action,
        Guid entityId,
        string? details = null,
        string? oldValues = null,
        string? newValues = null,
        AuditLogLevel level = AuditLogLevel.Info)
    {
        try
        {
            var dto = AuditLogFactory.ForAdmin(
                user: User,
                action: action,
                entityType: "ProductListing",
                entityId: entityId,
                oldValues: oldValues,
                newValues: newValues,
                details: details,
                level: level,
                ipAddress: HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                userAgent: Request?.Headers["User-Agent"].ToString());
            await _mediator.Send(new CreateAuditLogCommand(dto));
        }
        catch
        {
            // سجل التدقيق لا يجب أن يُفشل العملية الأساسية.
        }
    }
}

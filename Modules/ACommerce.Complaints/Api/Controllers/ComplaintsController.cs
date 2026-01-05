using System.Security.Claims;
using ACommerce.Complaints.DTOs;
using ACommerce.Complaints.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ACommerce.Complaints.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComplaintsController(IComplaintsService complaintsService) : ControllerBase
{
    #region Helper Methods

    private string? GetCurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    private string GetCurrentUserName() => User.FindFirst(ClaimTypes.Name)?.Value ?? "مستخدم";

    #endregion

    #region Complaints

    [HttpPost]
    public async Task<ActionResult<ComplaintResponseDto>> Create([FromBody] CreateComplaintDto request, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await complaintsService.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ComplaintResponseDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await complaintsService.GetByIdAsync(id, cancellationToken);
        if (result == null) return NotFound();

        // التحقق من أن المستخدم هو صاحب الشكوى
        var userId = GetCurrentUserId();
        if (result.UserId != userId)
            return Forbid();

        return Ok(result);
    }

    [HttpGet("ticket/{ticketNumber}")]
    public async Task<ActionResult<ComplaintResponseDto>> GetByTicketNumber(string ticketNumber, CancellationToken cancellationToken = default)
    {
        var result = await complaintsService.GetByTicketNumberAsync(ticketNumber, cancellationToken);
        if (result == null) return NotFound();

        var userId = GetCurrentUserId();
        if (result.UserId != userId)
            return Forbid();

        return Ok(result);
    }

    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<ComplaintSummaryDto>>> GetMyComplaints(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await complaintsService.GetByUserIdAsync(userId, status, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ComplaintResponseDto>> Update(Guid id, [FromBody] UpdateComplaintDto request, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await complaintsService.UpdateAsync(id, userId, request, cancellationToken);
        if (result == null) return NotFound();

        return Ok(result);
    }

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, [FromBody] CloseComplaintDto? request = null, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var success = await complaintsService.CloseAsync(id, userId, request, cancellationToken);
        if (!success) return NotFound();

        return Ok();
    }

    [HttpPost("{id:guid}/reopen")]
    public async Task<IActionResult> Reopen(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var success = await complaintsService.ReopenAsync(id, userId, cancellationToken);
        if (!success) return NotFound();

        return Ok();
    }

    [HttpPost("{id:guid}/rate")]
    public async Task<IActionResult> Rate(Guid id, [FromBody] RateComplaintDto request, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var success = await complaintsService.RateAsync(id, userId, request, cancellationToken);
        if (!success) return NotFound();

        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var success = await complaintsService.DeleteAsync(id, userId, cancellationToken);
        if (!success) return NotFound();

        return NoContent();
    }

    #endregion

    #region Replies

    [HttpGet("{complaintId:guid}/replies")]
    public async Task<ActionResult<IEnumerable<ComplaintReplyResponseDto>>> GetReplies(Guid complaintId, CancellationToken cancellationToken = default)
    {
        // تحقق من أن الشكوى تخص المستخدم
        var complaint = await complaintsService.GetByIdAsync(complaintId, cancellationToken);
        if (complaint == null) return NotFound();

        var userId = GetCurrentUserId();
        if (complaint.UserId != userId)
            return Forbid();

        var result = await complaintsService.GetRepliesAsync(complaintId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{complaintId:guid}/replies")]
    public async Task<ActionResult<ComplaintReplyResponseDto>> AddReply(Guid complaintId, [FromBody] CreateComplaintReplyDto request, CancellationToken cancellationToken = default)
    {
        // تحقق من أن الشكوى تخص المستخدم
        var complaint = await complaintsService.GetByIdAsync(complaintId, cancellationToken);
        if (complaint == null) return NotFound();

        var userId = GetCurrentUserId();
        var userName = GetCurrentUserName();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (complaint.UserId != userId)
            return Forbid();

        var result = await complaintsService.AddReplyAsync(complaintId, userId, userName, request, cancellationToken);
        return Ok(result);
    }

    #endregion

    #region Stats

    [HttpGet("me/stats")]
    public async Task<ActionResult<ComplaintStatsDto>> GetMyStats(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await complaintsService.GetUserStatsAsync(userId, cancellationToken);
        return Ok(result);
    }

    #endregion
}

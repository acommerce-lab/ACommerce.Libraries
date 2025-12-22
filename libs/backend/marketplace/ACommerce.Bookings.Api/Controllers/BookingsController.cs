using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ACommerce.Bookings.Entities;
using ACommerce.Bookings.DTOs;
using ACommerce.Bookings.Enums;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.Bookings.Api.Controllers;

/// <summary>
/// متحكم الحجوزات
/// </summary>
public class BookingsController(
    IMediator mediator,
    ILogger<BookingsController> logger)
    : BaseCrudController<Booking, CreateBookingDto, UpdateBookingDto, BookingResponseDto, UpdateBookingDto>(mediator, logger)
{
    /// <summary>
    /// الحصول على حجوزات المستأجر
    /// </summary>
    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<PagedResult<BookingResponseDto>>> GetCustomerBookings(
        string customerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var searchRequest = new SmartSearchRequest
            {
                PageSize = pageSize,
                PageNumber = pageNumber,
                Filters =
                [
                    new() { PropertyName = "CustomerId", Value = customerId, Operator = FilterOperator.Equals }
                ],
                OrderBy = "CreatedAt",
                Ascending = false
            };

            var query = new SmartSearchQuery<Booking, BookingResponseDto> { Request = searchRequest };
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings for customer {CustomerId}", customerId);
            return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
        }
    }

    /// <summary>
    /// الحصول على حجوزات المالك
    /// </summary>
    [HttpGet("host/{hostId}")]
    public async Task<ActionResult<PagedResult<BookingResponseDto>>> GetHostBookings(
        Guid hostId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var searchRequest = new SmartSearchRequest
            {
                PageSize = pageSize,
                PageNumber = pageNumber,
                Filters =
                [
                    new() { PropertyName = "HostId", Value = hostId.ToString(), Operator = FilterOperator.Equals }
                ],
                OrderBy = "CreatedAt",
                Ascending = false
            };

            var query = new SmartSearchQuery<Booking, BookingResponseDto> { Request = searchRequest };
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings for host {HostId}", hostId);
            return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
        }
    }

    /// <summary>
    /// الحصول على حجوزات عقار معين
    /// </summary>
    [HttpGet("space/{spaceId}")]
    public async Task<ActionResult<PagedResult<BookingResponseDto>>> GetSpaceBookings(
        Guid spaceId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var searchRequest = new SmartSearchRequest
            {
                PageSize = pageSize,
                PageNumber = pageNumber,
                Filters =
                [
                    new() { PropertyName = "SpaceId", Value = spaceId.ToString(), Operator = FilterOperator.Equals }
                ],
                OrderBy = "CheckInDate",
                Ascending = true
            };

            var query = new SmartSearchQuery<Booking, BookingResponseDto> { Request = searchRequest };
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings for space {SpaceId}", spaceId);
            return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
        }
    }

    /// <summary>
    /// التحقق من دفع العربون
    /// </summary>
    [HttpPost("{id}/verify-deposit")]
    public async Task<IActionResult> VerifyDepositPayment(Guid id, [FromBody] VerifyDepositPaymentDto dto)
    {
        try
        {
            // TODO: Implement payment verification logic
            // 1. Get booking
            // 2. Verify payment with payment provider
            // 3. Update booking status to DepositPaid
            // 4. Update escrow status to Held

            _logger.LogInformation("Verifying deposit payment for booking {BookingId}", id);

            return Ok(new { success = true, message = "تم التحقق من دفع العربون بنجاح" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying deposit for booking {BookingId}", id);
            return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
        }
    }

    /// <summary>
    /// تأكيد الحجز (من المالك)
    /// </summary>
    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> ConfirmBooking(Guid id, [FromBody] ConfirmBookingDto? dto)
    {
        try
        {
            // TODO: Implement confirmation logic
            // 1. Get booking and verify status is DepositPaid
            // 2. Verify caller is the host
            // 3. Update status to Confirmed
            // 4. Send notification to customer

            _logger.LogInformation("Confirming booking {BookingId}", id);

            return Ok(new { success = true, message = "تم تأكيد الحجز بنجاح" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming booking {BookingId}", id);
            return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
        }
    }

    /// <summary>
    /// رفض الحجز (من المالك)
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectBooking(Guid id, [FromBody] RejectBookingDto dto)
    {
        try
        {
            // TODO: Implement rejection logic
            // 1. Get booking and verify status
            // 2. Verify caller is the host
            // 3. Update status to Rejected
            // 4. Initiate refund if deposit was paid
            // 5. Send notification to customer

            _logger.LogInformation("Rejecting booking {BookingId} with reason: {Reason}", id, dto.Reason);

            return Ok(new { success = true, message = "تم رفض الحجز" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting booking {BookingId}", id);
            return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
        }
    }

    /// <summary>
    /// إلغاء الحجز
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid id, [FromBody] CancelBookingDto dto)
    {
        try
        {
            // TODO: Implement cancellation logic
            // 1. Get booking and verify it can be cancelled
            // 2. Determine refund amount based on cancellation policy
            // 3. Update status to Cancelled
            // 4. Initiate refund
            // 5. Send notifications

            _logger.LogInformation("Cancelling booking {BookingId} with reason: {Reason}", id, dto.Reason);

            return Ok(new { success = true, message = "تم إلغاء الحجز" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling booking {BookingId}", id);
            return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
        }
    }

    /// <summary>
    /// تحرير الضمان للمالك
    /// </summary>
    [HttpPost("{id}/release-escrow")]
    public async Task<IActionResult> ReleaseEscrow(Guid id, [FromBody] ReleaseEscrowDto? dto)
    {
        try
        {
            // TODO: Implement escrow release logic
            // 1. Get booking and verify status is Completed or approved for release
            // 2. Transfer funds to host
            // 3. Update escrow status to Released

            _logger.LogInformation("Releasing escrow for booking {BookingId}", id);

            return Ok(new { success = true, message = "تم تحرير الضمان" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing escrow for booking {BookingId}", id);
            return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
        }
    }

    /// <summary>
    /// استرداد الضمان للمستأجر
    /// </summary>
    [HttpPost("{id}/refund-escrow")]
    public async Task<IActionResult> RefundEscrow(Guid id, [FromBody] RefundEscrowDto dto)
    {
        try
        {
            // TODO: Implement escrow refund logic
            // 1. Get booking and verify it can be refunded
            // 2. Initiate refund to customer
            // 3. Update escrow status to Refunded

            _logger.LogInformation("Refunding escrow for booking {BookingId} with reason: {Reason}", id, dto.Reason);

            return Ok(new { success = true, message = "تم استرداد المبلغ" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding escrow for booking {BookingId}", id);
            return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
        }
    }
}

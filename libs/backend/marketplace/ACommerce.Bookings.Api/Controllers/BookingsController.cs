using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ACommerce.Bookings.Entities;
using ACommerce.Bookings.DTOs;
using ACommerce.Bookings.Enums;
using ACommerce.Marketing.Analytics.Services;
using ACommerce.SharedKernel.AspNetCore.Controllers;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.SharedKernel.CQRS.Queries;

namespace ACommerce.Bookings.Api.Controllers;

/// <summary>
/// متحكم الحجوزات
/// </summary>
public class BookingsController(
    IMediator mediator,
    IMarketingEventTracker marketingTracker,
    IBaseAsyncRepository<Booking> bookingRepository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<BookingsController> logger)
    : BaseCrudController<Booking, CreateBookingDto, UpdateBookingDto, BookingResponseDto, UpdateBookingDto>(mediator, logger)
{
    private readonly IMarketingEventTracker _marketingTracker = marketingTracker;
    private readonly IBaseAsyncRepository<Booking> _bookingRepository = bookingRepository;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
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
            // أمان: التحقق من أن customerId ليس فارغاً
            if (string.IsNullOrWhiteSpace(customerId))
            {
                _logger.LogWarning("GetCustomerBookings called with empty customerId");
                return BadRequest(new { message = "معرف العميل مطلوب" });
            }

            _logger.LogInformation("Getting bookings for customer: {CustomerId}", customerId);

            // DEBUG: Get all bookings to compare CustomerIds
            var allBookings = await _bookingRepository.GetPagedAsync(
                pageNumber: 1,
                pageSize: 100,
                orderBy: b => b.CreatedAt,
                ascending: false
            );

            // تجميع معلومات التشخيص لإرجاعها في الاستجابة
            var debugInfo = new
            {
                RequestedCustomerId = customerId,
                RequestedCustomerIdLength = customerId.Length,
                TotalBookingsInDb = allBookings.TotalCount,
                AllBookings = allBookings.Items.Take(20).Select(b => new
                {
                    BookingId = b.Id,
                    StoredCustomerId = b.CustomerId,
                    StoredCustomerIdLength = b.CustomerId?.Length ?? 0,
                    SpaceName = b.SpaceName,
                    CreatedAt = b.CreatedAt,
                    IsMatch = b.CustomerId == customerId
                }).ToList()
            };

            // استخدام repository مباشرة مع predicate للتأكد من الفلترة الصحيحة
            var result = await _bookingRepository.GetPagedAsync(
                pageNumber: pageNumber,
                pageSize: pageSize,
                predicate: b => b.CustomerId == customerId,
                orderBy: b => b.CreatedAt,
                ascending: false
            );

            _logger.LogInformation("Found {Count} bookings for customer {CustomerId}", result.TotalCount, customerId);

            // تحويل النتائج إلى DTOs
            var dtoItems = result.Items.Select(b => new BookingResponseDto
            {
                Id = b.Id,
                SpaceId = b.SpaceId,
                CustomerId = b.CustomerId,
                HostId = b.HostId,
                SpaceName = b.SpaceName,
                SpaceImage = b.SpaceImage,
                SpaceLocation = b.SpaceLocation,
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                RentType = b.RentType.ToString(),
                TotalPrice = b.TotalPrice,
                DepositPercentage = b.DepositPercentage,
                DepositAmount = b.DepositAmount,
                RemainingAmount = b.RemainingAmount,
                Currency = b.Currency,
                DepositPaymentId = b.DepositPaymentId,
                DepositPaidAt = b.DepositPaidAt,
                Status = b.Status.ToString(),
                EscrowStatus = b.EscrowStatus.ToString(),
                EscrowReleasedAt = b.EscrowReleasedAt,
                ConfirmedAt = b.ConfirmedAt,
                CancelledAt = b.CancelledAt,
                CancellationReason = b.CancellationReason,
                CustomerNotes = b.CustomerNotes,
                GuestsCount = b.GuestsCount,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            }).ToList();

            // إرجاع النتائج مع معلومات التشخيص
            return Ok(new
            {
                Items = dtoItems,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                Debug = debugInfo // معلومات التشخيص
            });
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
            // جلب الحجز من قاعدة البيانات
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound(new { success = false, message = "الحجز غير موجود" });
            }

            // التحقق من الحالة الحالية
            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.DepositPaid)
            {
                return BadRequest(new { success = false, message = "لا يمكن تأكيد هذا الحجز بحالته الحالية" });
            }

            _logger.LogInformation("Confirming booking {BookingId}", id);

            // تحديث حالة الحجز
            booking.Status = BookingStatus.Confirmed;
            booking.ConfirmedAt = DateTime.UtcNow;
            booking.HostNotes = dto?.HostNotes;
            booking.UpdatedAt = DateTime.UtcNow;

            await _bookingRepository.UpdateAsync(booking);

            // تتبع حدث الشراء (Purchase) عند تأكيد الحجز
            try
            {
                // Create user context with attribution data from headers
                var userContext = AttributionHeaderReader.CreateFromRequest(
                    _httpContextAccessor.HttpContext!,
                    booking.CustomerId);

                _logger.LogInformation("📊 Booking confirmed! Attribution: Fbc={Fbc}, Fbp={Fbp}",
                    userContext.Fbc ?? "(none)", userContext.Fbp ?? "(none)");

                await _marketingTracker.TrackPurchaseAsync(new PurchaseTrackingRequest
                {
                    TransactionId = id.ToString(),
                    Value = booking.TotalPrice,
                    Currency = booking.Currency,
                    ContentName = booking.SpaceName,
                    ContentIds = new[] { booking.SpaceId.ToString() },
                    ContentType = "booking",
                    User = userContext
                });
            }
            catch (Exception trackEx)
            {
                _logger.LogWarning(trackEx, "فشل تتبع حدث تأكيد الحجز");
            }

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
            // جلب الحجز من قاعدة البيانات
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound(new { success = false, message = "الحجز غير موجود" });
            }

            // التحقق من الحالة الحالية
            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.DepositPaid)
            {
                return BadRequest(new { success = false, message = "لا يمكن رفض هذا الحجز بحالته الحالية" });
            }

            _logger.LogInformation("Rejecting booking {BookingId} with reason: {Reason}", id, dto.Reason);

            // تحديث حالة الحجز
            booking.Status = BookingStatus.Rejected;
            booking.RejectedAt = DateTime.UtcNow;
            booking.RejectionReason = dto.Reason;
            booking.UpdatedAt = DateTime.UtcNow;

            await _bookingRepository.UpdateAsync(booking);

            // TODO: إرسال إشعار للعميل
            // TODO: بدء عملية استرداد العربون إذا تم دفعه

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

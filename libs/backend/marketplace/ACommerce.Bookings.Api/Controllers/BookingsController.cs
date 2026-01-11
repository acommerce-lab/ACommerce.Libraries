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
using ACommerce.Catalog.Listings.Entities;

namespace ACommerce.Bookings.Api.Controllers;

/// <summary>
/// متحكم الحجوزات
/// </summary>
public class BookingsController(
    IMediator mediator,
    IMarketingEventTracker marketingTracker,
    IBaseAsyncRepository<Booking> bookingRepository,
    IBaseAsyncRepository<ProductListing> listingRepository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<BookingsController> logger)
    : BaseCrudController<Booking, CreateBookingDto, UpdateBookingDto, BookingResponseDto, UpdateBookingDto>(mediator, logger)
{
    private readonly IMarketingEventTracker _marketingTracker = marketingTracker;
    private readonly IBaseAsyncRepository<Booking> _bookingRepository = bookingRepository;
    private readonly IBaseAsyncRepository<ProductListing> _listingRepository = listingRepository;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// إنشاء حجز جديد - يستخرج HostId تلقائياً من العقار
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BookingResponseDto>> CreateBooking([FromBody] CreateBookingDto dto)
    {
        try
        {
            _logger.LogInformation("Creating booking for space {SpaceId}, customer {CustomerId}", dto.SpaceId, dto.CustomerId);

            // استخراج HostId من العقار
            var listing = await _listingRepository.GetByIdAsync(dto.SpaceId);
            var hostId = listing?.VendorId ?? dto.HostId ?? Guid.Empty;

            _logger.LogInformation("Listing found: {ListingFound}, VendorId: {VendorId}, Final HostId: {HostId}",
                listing != null, listing?.VendorId, hostId);

            // إنشاء الحجز
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                SpaceId = dto.SpaceId,
                CustomerId = dto.CustomerId ?? "",
                HostId = hostId,
                SpaceName = listing?.Title,
                SpaceImage = listing?.MainImage,
                SpaceLocation = listing?.Address,
                CheckInDate = dto.CheckInDate,
                CheckOutDate = dto.CheckOutDate,
                RentType = dto.RentType,
                TotalPrice = dto.TotalPrice,
                DepositPercentage = dto.DepositPercentage ?? 10m,
                DepositAmount = dto.TotalPrice * (dto.DepositPercentage ?? 10m) / 100m,
                CustomerNotes = dto.CustomerNotes,
                GuestsCount = dto.GuestsCount,
                DepositPaymentId = dto.PaymentId,
                DepositPaidAt = !string.IsNullOrEmpty(dto.PaymentId) ? DateTime.UtcNow : null,
                Status = BookingStatus.Pending,
                EscrowStatus = EscrowStatus.None,
                CreatedAt = DateTime.UtcNow
            };

            await _bookingRepository.AddAsync(booking);

            _logger.LogInformation("Booking created: {BookingId} with HostId: {HostId}", booking.Id, booking.HostId);

            return Ok(new BookingResponseDto
            {
                Id = booking.Id,
                SpaceId = booking.SpaceId,
                CustomerId = booking.CustomerId,
                HostId = booking.HostId,
                SpaceName = booking.SpaceName,
                SpaceImage = booking.SpaceImage,
                SpaceLocation = booking.SpaceLocation,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                RentType = booking.RentType.ToString(),
                TotalPrice = booking.TotalPrice,
                DepositPercentage = booking.DepositPercentage,
                DepositAmount = booking.DepositAmount,
                RemainingAmount = booking.RemainingAmount,
                Currency = booking.Currency,
                Status = booking.Status.ToString(),
                EscrowStatus = booking.EscrowStatus.ToString(),
                CreatedAt = booking.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking for space {SpaceId}", dto.SpaceId);
            return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
        }
    }

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

            return Ok(new PagedResult<BookingResponseDto>
            {
                Items = dtoItems,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
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

    /// <summary>
    /// تحديث معرفات الحجز (للإدارة فقط)
    /// </summary>
    [HttpPost("{id}/update-ownership")]
    public async Task<IActionResult> UpdateBookingOwnership(Guid id, [FromBody] UpdateBookingOwnershipDto dto)
    {
        try
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound(new { success = false, message = "الحجز غير موجود" });
            }

            _logger.LogInformation("Updating booking {BookingId} ownership - CustomerId: {CustomerId}, HostId: {HostId}",
                id, dto.CustomerId, dto.HostId);

            if (!string.IsNullOrEmpty(dto.CustomerId))
            {
                booking.CustomerId = dto.CustomerId;
            }

            if (dto.HostId.HasValue)
            {
                booking.HostId = dto.HostId.Value;
            }

            booking.UpdatedAt = DateTime.UtcNow;
            await _bookingRepository.UpdateAsync(booking);

            return Ok(new
            {
                success = true,
                message = "تم تحديث معرفات الحجز",
                booking = new
                {
                    booking.Id,
                    booking.CustomerId,
                    booking.HostId,
                    booking.SpaceName
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating booking ownership {BookingId}", id);
            return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
        }
    }

    /// <summary>
    /// تحديث جميع الحجوزات (للإدارة - تغيير CustomerId)
    /// </summary>
    [HttpPost("admin/bulk-update-customer")]
    public async Task<IActionResult> BulkUpdateCustomerId([FromBody] BulkUpdateCustomerDto dto)
    {
        try
        {
            var allBookings = await _bookingRepository.GetPagedAsync(
                pageNumber: 1,
                pageSize: 100,
                predicate: b => b.CustomerId == dto.OldCustomerId,
                orderBy: b => b.CreatedAt,
                ascending: false
            );

            _logger.LogInformation("Found {Count} bookings to update from {Old} to {New}",
                allBookings.TotalCount, dto.OldCustomerId, dto.NewCustomerId);

            var updatedCount = 0;
            foreach (var booking in allBookings.Items)
            {
                booking.CustomerId = dto.NewCustomerId;
                if (dto.NewHostId.HasValue)
                {
                    booking.HostId = dto.NewHostId.Value;
                }
                booking.UpdatedAt = DateTime.UtcNow;
                await _bookingRepository.UpdateAsync(booking);
                updatedCount++;
            }

            return Ok(new
            {
                success = true,
                message = $"تم تحديث {updatedCount} حجز",
                updatedCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating bookings");
            return StatusCode(500, new { message = "An error occurred", detail = ex.Message });
        }
    }
}

/// <summary>
/// DTO لتحديث ملكية الحجز
/// </summary>
public class UpdateBookingOwnershipDto
{
    public string? CustomerId { get; set; }
    public Guid? HostId { get; set; }
}

/// <summary>
/// DTO للتحديث الجماعي
/// </summary>
public class BulkUpdateCustomerDto
{
    public required string OldCustomerId { get; set; }
    public required string NewCustomerId { get; set; }
    public Guid? NewHostId { get; set; }
}

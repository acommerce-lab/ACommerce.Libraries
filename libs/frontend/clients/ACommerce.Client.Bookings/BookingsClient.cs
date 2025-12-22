using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Bookings;

/// <summary>
/// Client للتعامل مع Bookings - حجوزات العقارات
/// </summary>
public sealed class BookingsClient
{
    private readonly IApiClient _httpClient;
    private const string ServiceName = "Marketplace";
    private const string BasePath = "/api/bookings";

    public BookingsClient(IApiClient httpClient)
    {
        _httpClient = httpClient;
    }

    // ═══════════════════════════════════════════════════════════════════
    // البحث والاستعلام
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// البحث في الحجوزات (SmartSearch)
    /// </summary>
    public async Task<PagedBookingResult?> SearchAsync(
        BookingSearchRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        request ??= new BookingSearchRequest();
        return await _httpClient.PostAsync<BookingSearchRequest, PagedBookingResult>(
            ServiceName,
            $"{BasePath}/search",
            request,
            cancellationToken);
    }

    /// <summary>
    /// الحصول على حجز محدد
    /// </summary>
    public async Task<BookingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<BookingDto>(
            ServiceName,
            $"{BasePath}/{id}",
            cancellationToken);
    }

    /// <summary>
    /// الحصول على حجوزاتي (كمستأجر)
    /// </summary>
    public async Task<PagedBookingResult?> GetMyBookingsAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<PagedBookingResult>(
            ServiceName,
            $"{BasePath}/customer/{customerId}",
            cancellationToken);
    }

    /// <summary>
    /// الحصول على الحجوزات الواردة (كمالك)
    /// </summary>
    public async Task<PagedBookingResult?> GetHostBookingsAsync(
        Guid hostId,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<PagedBookingResult>(
            ServiceName,
            $"{BasePath}/host/{hostId}",
            cancellationToken);
    }

    /// <summary>
    /// الحصول على حجوزات عقار معين
    /// </summary>
    public async Task<PagedBookingResult?> GetSpaceBookingsAsync(
        Guid spaceId,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<PagedBookingResult>(
            ServiceName,
            $"{BasePath}/space/{spaceId}",
            cancellationToken);
    }

    // ═══════════════════════════════════════════════════════════════════
    // إنشاء وتعديل
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// إنشاء حجز جديد
    /// </summary>
    public async Task<BookingDto?> CreateAsync(
        CreateBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.PostAsync<CreateBookingRequest, BookingDto>(
            ServiceName,
            BasePath,
            request,
            cancellationToken);
    }

    /// <summary>
    /// تحديث حجز
    /// </summary>
    public async Task UpdateAsync(
        Guid id,
        UpdateBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        await _httpClient.PutAsync<UpdateBookingRequest>(
            ServiceName,
            $"{BasePath}/{id}",
            request,
            cancellationToken);
    }

    // ═══════════════════════════════════════════════════════════════════
    // عمليات الدفع والتحقق
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// التحقق من دفع العربون
    /// </summary>
    public async Task<BookingActionResult?> VerifyDepositAsync(
        Guid bookingId,
        VerifyDepositRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.PostAsync<VerifyDepositRequest, BookingActionResult>(
            ServiceName,
            $"{BasePath}/{bookingId}/verify-deposit",
            request,
            cancellationToken);
    }

    // ═══════════════════════════════════════════════════════════════════
    // عمليات الحالة
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// تأكيد الحجز (من المالك)
    /// </summary>
    public async Task<BookingActionResult?> ConfirmAsync(
        Guid bookingId,
        ConfirmBookingRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.PostAsync<ConfirmBookingRequest?, BookingActionResult>(
            ServiceName,
            $"{BasePath}/{bookingId}/confirm",
            request,
            cancellationToken);
    }

    /// <summary>
    /// رفض الحجز (من المالك)
    /// </summary>
    public async Task<BookingActionResult?> RejectAsync(
        Guid bookingId,
        RejectBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.PostAsync<RejectBookingRequest, BookingActionResult>(
            ServiceName,
            $"{BasePath}/{bookingId}/reject",
            request,
            cancellationToken);
    }

    /// <summary>
    /// إلغاء الحجز
    /// </summary>
    public async Task<BookingActionResult?> CancelAsync(
        Guid bookingId,
        CancelBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.PostAsync<CancelBookingRequest, BookingActionResult>(
            ServiceName,
            $"{BasePath}/{bookingId}/cancel",
            request,
            cancellationToken);
    }

    // ═══════════════════════════════════════════════════════════════════
    // عمليات الضمان (Escrow)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// تحرير الضمان للمالك
    /// </summary>
    public async Task<BookingActionResult?> ReleaseEscrowAsync(
        Guid bookingId,
        ReleaseEscrowRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.PostAsync<ReleaseEscrowRequest?, BookingActionResult>(
            ServiceName,
            $"{BasePath}/{bookingId}/release-escrow",
            request,
            cancellationToken);
    }

    /// <summary>
    /// استرداد الضمان للمستأجر
    /// </summary>
    public async Task<BookingActionResult?> RefundEscrowAsync(
        Guid bookingId,
        RefundEscrowRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.PostAsync<RefundEscrowRequest, BookingActionResult>(
            ServiceName,
            $"{BasePath}/{bookingId}/refund-escrow",
            request,
            cancellationToken);
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// Models - نماذج البيانات
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// نتيجة البحث المقسمة لصفحات
/// </summary>
public sealed class PagedBookingResult
{
    public List<BookingDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// طلب البحث في الحجوزات
/// </summary>
public sealed class BookingSearchRequest
{
    public string? SearchTerm { get; set; }
    public List<BookingFilterItem>? Filters { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? OrderBy { get; set; }
    public bool Ascending { get; set; } = true;
    public List<string>? IncludeProperties { get; set; }
    public bool IncludeDeleted { get; set; }
}

/// <summary>
/// عنصر فلترة
/// </summary>
public sealed class BookingFilterItem
{
    public string PropertyName { get; set; } = string.Empty;
    public object? Value { get; set; }
    public object? SecondValue { get; set; }
    public int Operator { get; set; }
}

/// <summary>
/// Booking DTO للعرض
/// </summary>
public sealed class BookingDto
{
    public Guid Id { get; set; }

    // العلاقات
    public Guid SpaceId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public Guid HostId { get; set; }

    // معلومات العقار
    public string? SpaceName { get; set; }
    public string? SpaceImage { get; set; }
    public string? SpaceLocation { get; set; }

    // التواريخ
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public string RentType { get; set; } = "Monthly";

    // التسعير
    public decimal TotalPrice { get; set; }
    public decimal DepositPercentage { get; set; }
    public decimal DepositAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string Currency { get; set; } = "SAR";

    // الدفع
    public string? DepositPaymentId { get; set; }
    public DateTime? DepositPaidAt { get; set; }
    public bool IsDepositPaid => DepositPaidAt.HasValue;

    // الضمان
    public string EscrowStatus { get; set; } = "None";
    public DateTime? EscrowReleasedAt { get; set; }

    // الحالة
    public string Status { get; set; } = "Pending";
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    // معلومات إضافية
    public string? CustomerNotes { get; set; }
    public int GuestsCount { get; set; }
    public bool HasReview { get; set; }

    // التواريخ
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════════
// Request Models - نماذج الطلبات
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// إنشاء حجز
/// </summary>
public sealed class CreateBookingRequest
{
    public Guid SpaceId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public string RentType { get; set; } = "Monthly";
    public decimal TotalPrice { get; set; }
    public decimal? DepositPercentage { get; set; }
    public string? CustomerNotes { get; set; }
    public int GuestsCount { get; set; } = 1;
    public string? PaymentId { get; set; }
    public string? ReturnUrl { get; set; }
}

/// <summary>
/// تحديث حجز
/// </summary>
public sealed class UpdateBookingRequest
{
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public string? CustomerNotes { get; set; }
    public string? HostNotes { get; set; }
    public int? GuestsCount { get; set; }
}

/// <summary>
/// التحقق من الدفع
/// </summary>
public sealed class VerifyDepositRequest
{
    public required string PaymentId { get; set; }
    public string? TransactionId { get; set; }
}

/// <summary>
/// تأكيد الحجز
/// </summary>
public sealed class ConfirmBookingRequest
{
    public string? HostNotes { get; set; }
}

/// <summary>
/// رفض الحجز
/// </summary>
public sealed class RejectBookingRequest
{
    public required string Reason { get; set; }
}

/// <summary>
/// إلغاء الحجز
/// </summary>
public sealed class CancelBookingRequest
{
    public required string Reason { get; set; }
}

/// <summary>
/// تحرير الضمان
/// </summary>
public sealed class ReleaseEscrowRequest
{
    public decimal? Amount { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// استرداد الضمان
/// </summary>
public sealed class RefundEscrowRequest
{
    public decimal? Amount { get; set; }
    public required string Reason { get; set; }
}

/// <summary>
/// نتيجة عملية على الحجز
/// </summary>
public sealed class BookingActionResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public BookingDto? Booking { get; set; }
}

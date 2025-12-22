using ACommerce.Bookings.Enums;

namespace ACommerce.Bookings.DTOs;

/// <summary>
/// بيانات الحجز للعرض
/// </summary>
public class BookingResponseDto
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

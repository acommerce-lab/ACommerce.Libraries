using ACommerce.Spaces.Enums;

namespace ACommerce.Spaces.DTOs.Booking;

public class BookingResponseDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string BookingNumber { get; set; } = default!;
    public Guid SpaceId { get; set; }
    public string? SpaceName { get; set; }
    public string? SpaceImage { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int GuestsCount { get; set; }
    public BookingStatus Status { get; set; }
    public string StatusName { get; set; } = default!;
    public PricingType PricingType { get; set; }
    public string PricingTypeName { get; set; } = default!;
    public decimal BasePrice { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public string? DiscountCode { get; set; }
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; } = "SAR";
    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Purpose { get; set; }
    public string? CustomerNotes { get; set; }
    public string? SpecialRequests { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
    public bool IsReviewed { get; set; }
    public string? QrCode { get; set; }

    // Computed
    public int DurationMinutes => (int)(EndDateTime - StartDateTime).TotalMinutes;
    public bool CanCancel { get; set; }
    public bool CanModify { get; set; }
    public bool CanCheckIn { get; set; }
    public bool CanReview { get; set; }
}

using ACommerce.Spaces.Enums;

namespace ACommerce.Spaces.DTOs.Booking;

public class UpdateBookingDto
{
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int GuestsCount { get; set; }
    public string? Purpose { get; set; }
    public string? CustomerNotes { get; set; }
    public string? SpecialRequests { get; set; }
    public string? OwnerNotes { get; set; }
}

public class UpdateBookingStatusDto
{
    public BookingStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
}

public class CheckInOutDto
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}

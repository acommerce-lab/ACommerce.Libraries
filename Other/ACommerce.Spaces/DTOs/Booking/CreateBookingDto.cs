using ACommerce.Spaces.Enums;

namespace ACommerce.Spaces.DTOs.Booking;

public class CreateBookingDto
{
    public Guid SpaceId { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int GuestsCount { get; set; } = 1;
    public PricingType PricingType { get; set; }
    public string? Purpose { get; set; }
    public string? CustomerNotes { get; set; }
    public string? SpecialRequests { get; set; }
    public string? DiscountCode { get; set; }
    public string? BookingSource { get; set; }
}

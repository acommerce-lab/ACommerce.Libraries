using AutoMapper;
using ACommerce.Bookings.DTOs;
using ACommerce.Bookings.Entities;
using ACommerce.Bookings.Enums;

namespace ACommerce.Bookings.Mappings;

/// <summary>
/// AutoMapper Profile للحجوزات
/// </summary>
public class BookingMappingProfile : Profile
{
    public BookingMappingProfile()
    {
        // CreateBookingDto -> Booking
        CreateMap<CreateBookingDto, Booking>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId ?? ""))
            .ForMember(dest => dest.HostId, opt => opt.MapFrom(src => src.HostId ?? Guid.Empty))
            .ForMember(dest => dest.DepositPercentage, opt => opt.MapFrom(src => src.DepositPercentage ?? 10m))
            .ForMember(dest => dest.DepositAmount, opt => opt.MapFrom(src =>
                src.TotalPrice * (src.DepositPercentage ?? 10m) / 100m))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => BookingStatus.Pending))
            .ForMember(dest => dest.EscrowStatus, opt => opt.MapFrom(_ => EscrowStatus.None))
            .ForMember(dest => dest.DepositPaymentId, opt => opt.MapFrom(src => src.PaymentId))
            .ForMember(dest => dest.DepositPaidAt, opt => opt.MapFrom(src =>
                !string.IsNullOrEmpty(src.PaymentId) ? DateTime.UtcNow : (DateTime?)null))
            // Space info should be populated by custom handler
            .ForMember(dest => dest.SpaceName, opt => opt.Ignore())
            .ForMember(dest => dest.SpaceImage, opt => opt.Ignore())
            .ForMember(dest => dest.SpaceLocation, opt => opt.Ignore())
            // Ignored fields
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.FinalPaymentId, opt => opt.Ignore())
            .ForMember(dest => dest.FinalPaymentAt, opt => opt.Ignore())
            .ForMember(dest => dest.EscrowReleasedAt, opt => opt.Ignore())
            .ForMember(dest => dest.EscrowReleasedAmount, opt => opt.Ignore())
            .ForMember(dest => dest.ConfirmedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CancelledAt, opt => opt.Ignore())
            .ForMember(dest => dest.CancellationReason, opt => opt.Ignore())
            .ForMember(dest => dest.CancelledBy, opt => opt.Ignore())
            .ForMember(dest => dest.RejectedAt, opt => opt.Ignore())
            .ForMember(dest => dest.RejectionReason, opt => opt.Ignore())
            .ForMember(dest => dest.HostNotes, opt => opt.Ignore())
            .ForMember(dest => dest.InternalNotes, opt => opt.Ignore())
            .ForMember(dest => dest.ReviewId, opt => opt.Ignore());

        // UpdateBookingDto -> Booking
        CreateMap<UpdateBookingDto, Booking>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        // Booking -> BookingResponseDto
        CreateMap<Booking, BookingResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.RentType, opt => opt.MapFrom(src => src.RentType.ToString()))
            .ForMember(dest => dest.EscrowStatus, opt => opt.MapFrom(src => src.EscrowStatus.ToString()))
            .ForMember(dest => dest.IsDepositPaid, opt => opt.MapFrom(src => src.DepositPaidAt.HasValue))
            .ForMember(dest => dest.HasReview, opt => opt.MapFrom(src => src.ReviewId.HasValue));
    }
}

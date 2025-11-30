using ACommerce.Spaces.DTOs.Amenity;
using ACommerce.Spaces.DTOs.Booking;
using ACommerce.Spaces.DTOs.Review;
using ACommerce.Spaces.DTOs.Space;
using ACommerce.Spaces.Entities;
using AutoMapper;

namespace ACommerce.Spaces.Mappings;

public class SpaceMappingProfile : Profile
{
    public SpaceMappingProfile()
    {
        // Space mappings
        CreateMap<CreateSpaceDto, Space>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.AverageRating, opt => opt.Ignore())
            .ForMember(dest => dest.ReviewsCount, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedBookingsCount, opt => opt.Ignore())
            .ForMember(dest => dest.IsVerified, opt => opt.Ignore())
            .ForMember(dest => dest.SortOrder, opt => opt.Ignore())
            .ForMember(dest => dest.Amenities, opt => opt.Ignore())
            .ForMember(dest => dest.Prices, opt => opt.Ignore())
            .ForMember(dest => dest.Availabilities, opt => opt.Ignore())
            .ForMember(dest => dest.Bookings, opt => opt.Ignore())
            .ForMember(dest => dest.Reviews, opt => opt.Ignore());

        CreateMap<UpdateSpaceDto, Space>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
            .ForMember(dest => dest.AverageRating, opt => opt.Ignore())
            .ForMember(dest => dest.ReviewsCount, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedBookingsCount, opt => opt.Ignore())
            .ForMember(dest => dest.IsVerified, opt => opt.Ignore())
            .ForMember(dest => dest.SortOrder, opt => opt.Ignore())
            .ForMember(dest => dest.Amenities, opt => opt.Ignore())
            .ForMember(dest => dest.Prices, opt => opt.Ignore())
            .ForMember(dest => dest.Availabilities, opt => opt.Ignore())
            .ForMember(dest => dest.Bookings, opt => opt.Ignore())
            .ForMember(dest => dest.Reviews, opt => opt.Ignore());

        CreateMap<Space, SpaceResponseDto>()
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.AmenitiesCount, opt => opt.MapFrom(src => src.Amenities.Count))
            .ForMember(dest => dest.PricesCount, opt => opt.MapFrom(src => src.Prices.Count))
            .ForMember(dest => dest.MinPrice, opt => opt.MapFrom(src =>
                src.Prices.Any() ? src.Prices.Min(p => p.Price) : (decimal?)null))
            .ForMember(dest => dest.MaxPrice, opt => opt.MapFrom(src =>
                src.Prices.Any() ? src.Prices.Max(p => p.Price) : (decimal?)null))
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src =>
                src.Prices.FirstOrDefault() != null ? src.Prices.First().CurrencyCode : null));

        // Booking mappings
        CreateMap<CreateBookingDto, SpaceBooking>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.BookingNumber, opt => opt.Ignore())
            .ForMember(dest => dest.Space, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.BasePrice, opt => opt.Ignore())
            .ForMember(dest => dest.ServiceFee, opt => opt.Ignore())
            .ForMember(dest => dest.Tax, opt => opt.Ignore())
            .ForMember(dest => dest.Discount, opt => opt.Ignore())
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.CurrencyCode, opt => opt.Ignore())
            .ForMember(dest => dest.IsPaid, opt => opt.Ignore())
            .ForMember(dest => dest.PaidAt, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentTransactionId, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentMethod, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerNotes, opt => opt.Ignore())
            .ForMember(dest => dest.InternalNotes, opt => opt.Ignore())
            .ForMember(dest => dest.ConfirmedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CancelledAt, opt => opt.Ignore())
            .ForMember(dest => dest.CancellationReason, opt => opt.Ignore())
            .ForMember(dest => dest.CancelledBy, opt => opt.Ignore())
            .ForMember(dest => dest.RefundAmount, opt => opt.Ignore())
            .ForMember(dest => dest.RefundedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CheckedInAt, opt => opt.Ignore())
            .ForMember(dest => dest.CheckedOutAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsReviewed, opt => opt.Ignore())
            .ForMember(dest => dest.QrCode, opt => opt.Ignore())
            .ForMember(dest => dest.ReminderSent, opt => opt.Ignore())
            .ForMember(dest => dest.DeviceId, opt => opt.Ignore())
            .ForMember(dest => dest.IpAddress, opt => opt.Ignore());

        CreateMap<SpaceBooking, BookingResponseDto>()
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.PricingTypeName, opt => opt.MapFrom(src => src.PricingType.ToString()))
            .ForMember(dest => dest.SpaceName, opt => opt.MapFrom(src => src.Space != null ? src.Space.Name : null))
            .ForMember(dest => dest.SpaceImage, opt => opt.MapFrom(src => src.Space != null ? src.Space.FeaturedImage : null))
            .ForMember(dest => dest.CanCancel, opt => opt.Ignore())
            .ForMember(dest => dest.CanModify, opt => opt.Ignore())
            .ForMember(dest => dest.CanCheckIn, opt => opt.Ignore())
            .ForMember(dest => dest.CanReview, opt => opt.Ignore());

        // Review mappings
        CreateMap<CreateReviewDto, SpaceReview>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Space, opt => opt.Ignore())
            .ForMember(dest => dest.Booking, opt => opt.Ignore())
            .ForMember(dest => dest.ReviewerAvatar, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerResponse, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerResponseAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsVerified, opt => opt.Ignore())
            .ForMember(dest => dest.IsPublished, opt => opt.Ignore())
            .ForMember(dest => dest.HelpfulCount, opt => opt.Ignore())
            .ForMember(dest => dest.ReportCount, opt => opt.Ignore());

        CreateMap<SpaceReview, ReviewResponseDto>();

        // Amenity mappings
        CreateMap<CreateAmenityDto, SpaceAmenity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Space, opt => opt.Ignore());

        CreateMap<UpdateAmenityDto, SpaceAmenity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.SpaceId, opt => opt.Ignore())
            .ForMember(dest => dest.Space, opt => opt.Ignore());

        CreateMap<SpaceAmenity, AmenityResponseDto>();
    }
}

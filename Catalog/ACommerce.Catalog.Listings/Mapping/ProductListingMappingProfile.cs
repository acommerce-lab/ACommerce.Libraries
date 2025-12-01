using System.Text.Json;
using AutoMapper;
using ACommerce.Catalog.Listings.DTOs;
using ACommerce.Catalog.Listings.Entities;

namespace ACommerce.Catalog.Listings.Mapping;

/// <summary>
/// AutoMapper profile for ProductListing to handle JSON conversion
/// </summary>
public class ProductListingMappingProfile : Profile
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false
	};

	public ProductListingMappingProfile()
	{
		// CreateProductListingDto -> ProductListing
		CreateMap<CreateProductListingDto, ProductListing>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.Status, opt => opt.Ignore())
			.ForMember(dest => dest.QuantityReserved, opt => opt.Ignore())
			.ForMember(dest => dest.LowStockThreshold, opt => opt.Ignore())
			.ForMember(dest => dest.StartsAt, opt => opt.Ignore())
			.ForMember(dest => dest.EndsAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsActive, opt => opt.Ignore())
			.ForMember(dest => dest.TotalSales, opt => opt.Ignore())
			.ForMember(dest => dest.ViewCount, opt => opt.Ignore())
			.ForMember(dest => dest.Rating, opt => opt.Ignore())
			.ForMember(dest => dest.ReviewCount, opt => opt.Ignore())
			.ForMember(dest => dest.Metadata, opt => opt.Ignore())
			.ForMember(dest => dest.ImagesJson, opt => opt.MapFrom(src =>
				src.Images != null ? JsonSerializer.Serialize(src.Images, JsonOptions) : null))
			.ForMember(dest => dest.AttributesJson, opt => opt.MapFrom(src =>
				src.Attributes != null ? JsonSerializer.Serialize(src.Attributes, JsonOptions) : null));

		// ProductListing -> ProductListingResponseDto
		CreateMap<ProductListing, ProductListingResponseDto>();
	}
}

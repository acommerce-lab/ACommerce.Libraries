using ACommerce.Catalog.Products.DTOs.Product;
using ACommerce.Catalog.Products.Entities;
using AutoMapper;

namespace ACommerce.Catalog.Products.Mappings;

public class ProductMappingProfile : Profile
{
	public ProductMappingProfile()
	{
		// Product mappings
		CreateMap<CreateProductDto, Product>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.WeightUnit, opt => opt.Ignore())
			.ForMember(dest => dest.DimensionUnit, opt => opt.Ignore())
			.ForMember(dest => dest.ParentProduct, opt => opt.Ignore())
			.ForMember(dest => dest.Variants, opt => opt.Ignore())
			.ForMember(dest => dest.Categories, opt => opt.Ignore())
			.ForMember(dest => dest.Brands, opt => opt.Ignore())
			.ForMember(dest => dest.Attributes, opt => opt.Ignore())
			.ForMember(dest => dest.Prices, opt => opt.Ignore())
			.ForMember(dest => dest.Inventory, opt => opt.Ignore())
			.ForMember(dest => dest.RelatedProducts, opt => opt.Ignore())
			.ForMember(dest => dest.Reviews, opt => opt.Ignore());

		CreateMap<Product, ProductResponseDto>()
			.ForMember(dest => dest.WeightUnitName,
				opt => opt.MapFrom(src => src.WeightUnit != null ? src.WeightUnit.Name : null))
			.ForMember(dest => dest.WeightUnitSymbol,
				opt => opt.MapFrom(src => src.WeightUnit != null ? src.WeightUnit.Symbol : null))
			.ForMember(dest => dest.DimensionUnitName,
				opt => opt.MapFrom(src => src.DimensionUnit != null ? src.DimensionUnit.Name : null))
			.ForMember(dest => dest.DimensionUnitSymbol,
				opt => opt.MapFrom(src => src.DimensionUnit != null ? src.DimensionUnit.Symbol : null))
			.ForMember(dest => dest.VariantsCount,
				opt => opt.MapFrom(src => src.Variants.Count))
			.ForMember(dest => dest.CategoriesCount,
				opt => opt.MapFrom(src => src.Categories.Count))
			.ForMember(dest => dest.AttributesCount,
				opt => opt.MapFrom(src => src.Attributes.Count));

		// ProductAttribute mappings
		CreateMap<ProductAttribute, ProductAttributeDto>()
			.ForMember(dest => dest.AttributeName,
				opt => opt.MapFrom(src => src.AttributeDefinition != null ? src.AttributeDefinition.Name : string.Empty))
			.ForMember(dest => dest.AttributeCode,
				opt => opt.MapFrom(src => src.AttributeDefinition != null ? src.AttributeDefinition.Code : string.Empty))
			.ForMember(dest => dest.ValueName,
				opt => opt.MapFrom(src => src.AttributeValue != null ? src.AttributeValue.Value : src.CustomValue ?? string.Empty));

		// ProductPrice mappings
		CreateMap<ProductPrice, ProductPriceDto>()
			.ForMember(dest => dest.CurrencyCode,
				opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Code : string.Empty))
			.ForMember(dest => dest.CurrencySymbol,
				opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Symbol : string.Empty));
	}
}

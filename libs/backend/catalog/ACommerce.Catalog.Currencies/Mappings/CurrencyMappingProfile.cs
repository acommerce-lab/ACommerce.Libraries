using ACommerce.Catalog.Currencies.DTOs.Currency;
using ACommerce.Catalog.Currencies.DTOs.ExchangeRate;
using ACommerce.Catalog.Currencies.Entities;
using AutoMapper;

namespace ACommerce.Catalog.Currencies.Mappings;

public class CurrencyMappingProfile : Profile
{
	public CurrencyMappingProfile()
	{
		// Currency mappings
		CreateMap<CreateCurrencyDto, Currency>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.ExchangeRatesFrom, opt => opt.Ignore())
			.ForMember(dest => dest.ExchangeRatesTo, opt => opt.Ignore());

		CreateMap<Currency, CurrencyResponseDto>();

		// ExchangeRate mappings
		CreateMap<CreateExchangeRateDto, ExchangeRate>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.FromCurrency, opt => opt.Ignore())
			.ForMember(dest => dest.ToCurrency, opt => opt.Ignore());

		CreateMap<ExchangeRate, ExchangeRateResponseDto>()
			.ForMember(dest => dest.FromCurrencyCode,
				opt => opt.MapFrom(src => src.FromCurrency != null ? src.FromCurrency.Code : string.Empty))
			.ForMember(dest => dest.FromCurrencyName,
				opt => opt.MapFrom(src => src.FromCurrency != null ? src.FromCurrency.Name : string.Empty))
			.ForMember(dest => dest.FromCurrencySymbol,
				opt => opt.MapFrom(src => src.FromCurrency != null ? src.FromCurrency.Symbol : string.Empty))
			.ForMember(dest => dest.ToCurrencyCode,
				opt => opt.MapFrom(src => src.ToCurrency != null ? src.ToCurrency.Code : string.Empty))
			.ForMember(dest => dest.ToCurrencyName,
				opt => opt.MapFrom(src => src.ToCurrency != null ? src.ToCurrency.Name : string.Empty))
			.ForMember(dest => dest.ToCurrencySymbol,
				opt => opt.MapFrom(src => src.ToCurrency != null ? src.ToCurrency.Symbol : string.Empty));
	}
}

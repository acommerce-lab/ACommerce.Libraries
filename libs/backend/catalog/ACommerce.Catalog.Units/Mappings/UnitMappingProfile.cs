using ACommerce.Catalog.Units.DTOs.Unit;
using ACommerce.Catalog.Units.DTOs.MeasurementSystem;
using ACommerce.Catalog.Units.Entities;
using AutoMapper;
using ACommerce.Catalog.Units.DTOs.UnitCategory;

namespace ACommerce.Catalog.Units.Mappings;

public class UnitMappingProfile : Profile
{
	public UnitMappingProfile()
	{
		// Unit mappings
		CreateMap<CreateUnitDto, Unit>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.UnitCategory, opt => opt.Ignore())
			.ForMember(dest => dest.MeasurementSystem, opt => opt.Ignore())
			.ForMember(dest => dest.ConversionsFrom, opt => opt.Ignore())
			.ForMember(dest => dest.ConversionsTo, opt => opt.Ignore());

		CreateMap<Unit, UnitResponseDto>()
			.ForMember(dest => dest.UnitCategoryName,
				opt => opt.MapFrom(src => src.UnitCategory != null ? src.UnitCategory.Name : string.Empty))
			.ForMember(dest => dest.UnitCategoryCode,
				opt => opt.MapFrom(src => src.UnitCategory != null ? src.UnitCategory.Code : string.Empty))
			.ForMember(dest => dest.MeasurementSystemName,
				opt => opt.MapFrom(src => src.MeasurementSystem != null ? src.MeasurementSystem.Name : string.Empty))
			.ForMember(dest => dest.MeasurementSystemCode,
				opt => opt.MapFrom(src => src.MeasurementSystem != null ? src.MeasurementSystem.Code : string.Empty));

		// UnitCategory mappings
		CreateMap<CreateUnitCategoryDto, UnitCategory>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.BaseUnit, opt => opt.Ignore())
			.ForMember(dest => dest.Units, opt => opt.Ignore());

		CreateMap<UpdateUnitCategoryDto, UnitCategory>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.BaseUnit, opt => opt.Ignore())
			.ForMember(dest => dest.Units, opt => opt.Ignore())
			.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

		CreateMap<PartialUpdateUnitCategoryDto, UnitCategory>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.BaseUnit, opt => opt.Ignore())
			.ForMember(dest => dest.Units, opt => opt.Ignore())
			.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

		CreateMap<UnitCategory, UnitCategoryResponseDto>()
			.ForMember(dest => dest.BaseUnitName,
				opt => opt.MapFrom(src => src.BaseUnit != null ? src.BaseUnit.Name : null))
			.ForMember(dest => dest.BaseUnitSymbol,
				opt => opt.MapFrom(src => src.BaseUnit != null ? src.BaseUnit.Symbol : null))
			.ForMember(dest => dest.UnitsCount,
				opt => opt.MapFrom(src => src.Units.Count));

		// MeasurementSystem mappings
		CreateMap<CreateMeasurementSystemDto, MeasurementSystem>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.Units, opt => opt.Ignore());

		CreateMap<MeasurementSystem, MeasurementSystemResponseDto>()
			.ForMember(dest => dest.UnitsCount,
				opt => opt.MapFrom(src => src.Units.Count));
	}
}

using ACommerce.Catalog.Attributes.DTOs.AttributeDefinition;
using ACommerce.Catalog.Attributes.DTOs.AttributeValue;
using ACommerce.Catalog.Attributes.Entities;
using AutoMapper;

namespace ACommerce.Catalog.Attributes.Mappings;

public class AttributeMappingProfile : Profile
{
	public AttributeMappingProfile()
	{
		// AttributeDefinition
		CreateMap<CreateAttributeDefinitionDto, AttributeDefinition>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.Values, opt => opt.Ignore());

		CreateMap<UpdateAttributeDefinitionDto, AttributeDefinition>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.Code, opt => opt.Ignore())
			.ForMember(dest => dest.Type, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.Values, opt => opt.Ignore())
			.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

		CreateMap<PartialUpdateAttributeDefinitionDto, AttributeDefinition>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.Code, opt => opt.Ignore())
			.ForMember(dest => dest.Type, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.Values, opt => opt.Ignore())
			.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

		CreateMap<AttributeDefinition, AttributeDefinitionResponseDto>()
			.ForMember(dest => dest.ValuesCount, opt => opt.MapFrom(src => src.Values.Count));

		// AttributeValue
		CreateMap<CreateAttributeValueDto, AttributeValue>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
			.ForMember(dest => dest.AttributeDefinition, opt => opt.Ignore())
			.ForMember(dest => dest.ParentRelationships, opt => opt.Ignore())
			.ForMember(dest => dest.ChildRelationships, opt => opt.Ignore());

		CreateMap<UpdateAttributeValueDto, AttributeValue>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.AttributeDefinitionId, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.AttributeDefinition, opt => opt.Ignore())
			.ForMember(dest => dest.ParentRelationships, opt => opt.Ignore())
			.ForMember(dest => dest.ChildRelationships, opt => opt.Ignore())
			.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

		CreateMap<PartialUpdateAttributeValueDto, AttributeValue>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.AttributeDefinitionId, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.AttributeDefinition, opt => opt.Ignore())
			.ForMember(dest => dest.ParentRelationships, opt => opt.Ignore())
			.ForMember(dest => dest.ChildRelationships, opt => opt.Ignore())
			.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

		CreateMap<AttributeValue, AttributeValueResponseDto>()
			.ForMember(dest => dest.AttributeDefinitionName,
				opt => opt.MapFrom(src => src.AttributeDefinition != null ? src.AttributeDefinition.Name : string.Empty));
	}
}


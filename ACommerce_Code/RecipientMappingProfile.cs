using ACommerce.Notifications.Recipients.DTOs;
using ACommerce.Notifications.Recipients.DTOs.ContactPoint;
using ACommerce.Notifications.Recipients.DTOs.RecipientGroup;
using ACommerce.Notifications.Recipients.DTOs.UserRecipient;
using ACommerce.Notifications.Recipients.Entities;
using AutoMapper;

namespace ACommerce.Notifications.Recipients.Mappings;

/// <summary>
/// AutoMapper Profile ?????????
/// </summary>
public class RecipientMappingProfile : Profile
{
	public RecipientMappingProfile()
	{
		// ContactPoint Mappings
		CreateMap<CreateContactPointDto, ContactPoint>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.IsVerified, opt => opt.MapFrom(src => false))
			.ForMember(dest => dest.UserRecipientId, opt => opt.Ignore())
			.ForMember(dest => dest.UserRecipient, opt => opt.Ignore());

		CreateMap<UpdateContactPointDto, ContactPoint>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.UserId, opt => opt.Ignore())
			.ForMember(dest => dest.Type, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.UserRecipientId, opt => opt.Ignore())
			.ForMember(dest => dest.UserRecipient, opt => opt.Ignore())
			.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

		CreateMap<PartialUpdateContactPointDto, ContactPoint>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.UserId, opt => opt.Ignore())
			.ForMember(dest => dest.Type, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.UserRecipientId, opt => opt.Ignore())
			.ForMember(dest => dest.UserRecipient, opt => opt.Ignore())
			.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

		CreateMap<ContactPoint, ContactPointResponseDto>();

		// UserRecipient Mappings
		CreateMap<CreateUserRecipientDto, UserRecipient>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
			.ForMember(dest => dest.ContactPoints, opt => opt.MapFrom(src => src.ContactPoints ?? new List<CreateContactPointDto>()))
			.ForMember(dest => dest.Groups, opt => opt.Ignore());

		CreateMap<UpdateUserRecipientDto, UserRecipient>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.UserId, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.ContactPoints, opt => opt.Ignore())
			.ForMember(dest => dest.Groups, opt => opt.Ignore())
			.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

		CreateMap<PartialUpdateUserRecipientDto, UserRecipient>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.UserId, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.ContactPoints, opt => opt.Ignore())
			.ForMember(dest => dest.Groups, opt => opt.Ignore())
			.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

		CreateMap<UserRecipient, UserRecipientResponseDto>()
			.ForMember(dest => dest.ContactPoints, opt => opt.MapFrom(src => src.ContactPoints))
			.ForMember(dest => dest.Groups, opt => opt.MapFrom(src => src.Groups));

		// RecipientGroup Mappings
		CreateMap<CreateRecipientGroupDto, RecipientGroup>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
			.ForMember(dest => dest.Members, opt => opt.Ignore());

		CreateMap<UpdateRecipientGroupDto, RecipientGroup>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.Members, opt => opt.Ignore())
			.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

		CreateMap<PartialUpdateRecipientGroupDto, RecipientGroup>()
			.ForMember(dest => dest.Id, opt => opt.Ignore())
			.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
			.ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
			.ForMember(dest => dest.Members, opt => opt.Ignore())
			.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

		CreateMap<RecipientGroup, RecipientGroupResponseDto>()
			.ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.Members.Count));
	}
}


using AutoMapper;
using ACommerce.SharedKernel.Abstractions.Entities;
using System.Reflection;

namespace ACommerce.SharedKernel.CQRS.Mapping;

/// <summary>
/// Profile تلقائي يعتمد على Convention لتسمية الـ DTOs
/// </summary>
public class ConventionMappingProfile : Profile
{
	public ConventionMappingProfile()
	{
	}

	public ConventionMappingProfile(params Assembly[] assembliesToScan)
	{
		var assemblies = assembliesToScan?.Length > 0
			? assembliesToScan
			: AppDomain.CurrentDomain.GetAssemblies();

		var allTypes = assemblies
			.SelectMany(a =>
			{
				try
				{
					return a.GetTypes();
				}
				catch
				{
					return Array.Empty<Type>();
				}
			})
			.ToList();

		// CreateDto → Entity
		MapCreateDtos(allTypes);

		// UpdateDto → Entity
		MapUpdateDtos(allTypes);

		// PartialUpdateDto → Entity
		MapPartialUpdateDtos(allTypes);

		// Entity → ResponseDto
		MapResponseDtos(allTypes);
	}

	private void MapCreateDtos(List<Type> allTypes)
	{
		var createDtos = allTypes
			.Where(t => t.IsClass && !t.IsAbstract &&
						t.Name.StartsWith("Create") &&
						t.Name.EndsWith("Dto"))
			.ToList();

		foreach (var dto in createDtos)
		{
			var entityName = dto.Name.Substring("Create".Length, dto.Name.Length - "Create".Length - "Dto".Length);
			var entity = allTypes.FirstOrDefault(t =>
				t.IsClass &&
				typeof(IBaseEntity).IsAssignableFrom(t) &&
				t.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase));

			if (entity != null)
			{
				CreateMap(dto, entity)
					.ForMember("Id", opt => opt.Ignore())
					.ForMember("CreatedAt", opt => opt.Ignore())
					.ForMember("UpdatedAt", opt => opt.Ignore())
					.ForMember("IsDeleted", opt => opt.Ignore());
			}
		}
	}

	private void MapUpdateDtos(List<Type> allTypes)
	{
		var updateDtos = allTypes
			.Where(t => t.IsClass && !t.IsAbstract &&
						t.Name.StartsWith("Update") &&
						t.Name.EndsWith("Dto") &&
						!t.Name.Contains("Partial"))
			.ToList();

		foreach (var dto in updateDtos)
		{
			var entityName = dto.Name.Substring("Update".Length, dto.Name.Length - "Update".Length - "Dto".Length);
			var entity = allTypes.FirstOrDefault(t =>
				t.IsClass &&
				typeof(IBaseEntity).IsAssignableFrom(t) &&
				t.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase));

			if (entity != null)
			{
				CreateMap(dto, entity)
					.ForMember("Id", opt => opt.Ignore())
					.ForMember("CreatedAt", opt => opt.Ignore())
					.ForMember("UpdatedAt", opt => opt.Ignore())
					.ForMember("IsDeleted", opt => opt.Ignore())
					.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
			}
		}
	}

	private void MapPartialUpdateDtos(List<Type> allTypes)
	{
		var partialUpdateDtos = allTypes
			.Where(t => t.IsClass && !t.IsAbstract &&
						t.Name.Contains("PartialUpdate") &&
						t.Name.EndsWith("Dto"))
			.ToList();

		foreach (var dto in partialUpdateDtos)
		{
			var entityName = dto.Name
				.Replace("PartialUpdate", string.Empty)
				.Replace("Dto", string.Empty);

			var entity = allTypes.FirstOrDefault(t =>
				t.IsClass &&
				typeof(IBaseEntity).IsAssignableFrom(t) &&
				t.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase));

			if (entity != null)
			{
				CreateMap(dto, entity)
					.ForMember("Id", opt => opt.Ignore())
					.ForMember("CreatedAt", opt => opt.Ignore())
					.ForMember("UpdatedAt", opt => opt.Ignore())
					.ForMember("IsDeleted", opt => opt.Ignore())
					.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
			}
		}
	}

	private void MapResponseDtos(List<Type> allTypes)
	{
		var responseDtos = allTypes
			.Where(t => t.IsClass && !t.IsAbstract &&
						t.Name.Contains("Response") &&
						t.Name.EndsWith("Dto"))
			.ToList();

		foreach (var dto in responseDtos)
		{
			var entityName = dto.Name.Replace("ResponseDto", string.Empty);
			var entity = allTypes.FirstOrDefault(t =>
				t.IsClass &&
				typeof(IBaseEntity).IsAssignableFrom(t) &&
				t.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase));

			if (entity != null)
			{
				CreateMap(entity, dto).ReverseMap();
			}
		}
	}
}

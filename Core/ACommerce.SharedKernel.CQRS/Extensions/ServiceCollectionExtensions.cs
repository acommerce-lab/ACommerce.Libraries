using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Abstractions.Queries;
using ACommerce.SharedKernel.CQRS.Behaviors;
using ACommerce.SharedKernel.CQRS.Commands;
using ACommerce.SharedKernel.CQRS.Handlers;
using ACommerce.SharedKernel.CQRS.Mapping;
using ACommerce.SharedKernel.CQRS.Queries;
using System.Reflection;

namespace ACommerce.SharedKernel.CQRS.Extensions;

public static class ServiceCollectionExtensions
{
	/// <summary>
	/// إضافة CQRS مع MediatR و AutoMapper و FluentValidation
	/// </summary>
	public static IServiceCollection AddSharedKernelCQRS(
		this IServiceCollection services,
		params Assembly[] assemblies)
	{
		if (assemblies == null || assemblies.Length == 0)
		{
			assemblies = [Assembly.GetCallingAssembly()];
		}

		// MediatR
		services.AddMediatR(cfg =>
		{
			cfg.RegisterServicesFromAssemblies(assemblies);

			// Behaviors
			cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
			cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
			cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
		});

		// Auto-discover and register CQRS handlers for entities
		RegisterCqrsHandlersForEntities(services, assemblies);

		// AutoMapper
		services.AddAutoMapper(cfg =>
		{
			cfg.AddProfile(new ConventionMappingProfile(assemblies));
			cfg.AddMaps(assemblies);
		});

		// FluentValidation
		services.AddValidatorsFromAssemblies(assemblies);

		return services;
	}

	/// <summary>
	/// Auto-discover entities and their DTOs and register CQRS handlers
	/// </summary>
	private static void RegisterCqrsHandlersForEntities(IServiceCollection services, Assembly[] assemblies)
	{
		var entityInterface = typeof(IBaseEntity);

		foreach (var assembly in assemblies)
		{
			try
			{
				var entityTypes = assembly.GetTypes()
					.Where(t => entityInterface.IsAssignableFrom(t)
						&& t.IsClass
						&& !t.IsAbstract
						&& !t.IsGenericType)
					.ToList();

				foreach (var entityType in entityTypes)
				{
					// Look for matching ResponseDto in the same or related assemblies
					var responseDtoType = FindResponseDtoForEntity(entityType, assemblies);
					if (responseDtoType != null)
					{
						RegisterHandlersForEntity(services, entityType, responseDtoType);
					}
				}
			}
			catch (ReflectionTypeLoadException)
			{
				// Skip assemblies that can't be loaded
				continue;
			}
		}
	}

	/// <summary>
	/// Find a matching ResponseDto for an entity by naming convention
	/// </summary>
	private static Type? FindResponseDtoForEntity(Type entityType, Assembly[] assemblies)
	{
		var entityName = entityType.Name;
		var possibleDtoNames = new[]
		{
			$"{entityName}ResponseDto",
			$"{entityName}Dto",
			$"{entityName}Response"
		};

		foreach (var assembly in assemblies)
		{
			try
			{
				var dtoType = assembly.GetTypes()
					.FirstOrDefault(t => possibleDtoNames.Contains(t.Name)
						&& t.IsClass
						&& !t.IsAbstract);

				if (dtoType != null)
					return dtoType;
			}
			catch
			{
				continue;
			}
		}

		return null;
	}

	/// <summary>
	/// Register SmartSearchQuery and GetByIdQuery handlers for an entity
	/// </summary>
	private static void RegisterHandlersForEntity(IServiceCollection services, Type entityType, Type responseDtoType)
	{
		// SmartSearchQuery handler
		var smartSearchQueryType = typeof(SmartSearchQuery<,>).MakeGenericType(entityType, responseDtoType);
		var smartSearchResultType = typeof(PagedResult<>).MakeGenericType(responseDtoType);
		var smartSearchHandlerType = typeof(SmartSearchQueryHandler<,>).MakeGenericType(entityType, responseDtoType);
		var smartSearchRequestHandlerType = typeof(IRequestHandler<,>).MakeGenericType(smartSearchQueryType, smartSearchResultType);

		services.AddScoped(smartSearchRequestHandlerType, smartSearchHandlerType);

		// GetByIdQuery handler
		var getByIdQueryType = typeof(GetByIdQuery<,>).MakeGenericType(entityType, responseDtoType);
		var nullableResponseDtoType = responseDtoType; // The handler returns TDto? but we register with TDto
		var getByIdHandlerType = typeof(GetByIdQueryHandler<,>).MakeGenericType(entityType, responseDtoType);
		var getByIdRequestHandlerType = typeof(IRequestHandler<,>).MakeGenericType(getByIdQueryType, nullableResponseDtoType);

		services.AddScoped(getByIdRequestHandlerType, getByIdHandlerType);
	}

	/// <summary>
	/// تسجيل معالجات محددة لكيان معين
	/// </summary>
	public static IServiceCollection AddEntityHandlers<TEntity, TCreateDto, TUpdateDto, TResponseDto, TPartialUpdateDto>(
		this IServiceCollection services)
		where TEntity : class, IBaseEntity
	{
		// Commands
		services.AddScoped<IRequestHandler<CreateCommand<TEntity, TCreateDto>, TEntity>,
			CreateCommandHandler<TEntity, TCreateDto>>();

		services.AddScoped<IRequestHandler<UpdateCommand<TEntity, TUpdateDto>, Unit>,
			UpdateCommandHandler<TEntity, TUpdateDto>>();

		services.AddScoped<IRequestHandler<PartialUpdateCommand<TEntity, TPartialUpdateDto>, Unit>,
			PartialUpdateCommandHandler<TEntity, TPartialUpdateDto>>();

		services.AddScoped<IRequestHandler<DeleteCommand<TEntity>, Unit>,
			DeleteCommandHandler<TEntity>>();

		services.AddScoped<IRequestHandler<RestoreCommand<TEntity>, Unit>,
			RestoreCommandHandler<TEntity>>();

		// Queries
		services.AddScoped<IRequestHandler<GetByIdQuery<TEntity, TResponseDto>, TResponseDto?>,
			GetByIdQueryHandler<TEntity, TResponseDto>>();

		services.AddScoped<IRequestHandler<SmartSearchQuery<TEntity, TResponseDto>, PagedResult<TResponseDto>>,
			SmartSearchQueryHandler<TEntity, TResponseDto>>();

		return services;
	}
}

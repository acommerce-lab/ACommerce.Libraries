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
using System.Runtime.Loader;

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

		// Load all ACommerce assemblies from the application directory
		var allAssemblies = LoadACommerceAssemblies(assemblies);

		// MediatR
		services.AddMediatR(cfg =>
		{
			cfg.RegisterServicesFromAssemblies(allAssemblies);

			// Behaviors
			cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
			cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
			cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
		});

		// Auto-discover and register CQRS handlers for entities
		RegisterCqrsHandlersForEntities(services, allAssemblies);

		// AutoMapper
		services.AddAutoMapper(cfg =>
		{
			cfg.AddProfile(new ConventionMappingProfile(allAssemblies));
			cfg.AddMaps(allAssemblies);
		});

		// FluentValidation
		services.AddValidatorsFromAssemblies(allAssemblies);

		return services;
	}

	/// <summary>
	/// Load all ACommerce assemblies from the application directory
	/// </summary>
	private static Assembly[] LoadACommerceAssemblies(Assembly[] initialAssemblies)
	{
		var loadedAssemblies = new HashSet<Assembly>(initialAssemblies);

		// Get the base directory where the application is running
		var baseDirectory = AppContext.BaseDirectory;

		try
		{
			// Find all ACommerce DLLs in the base directory
			var acommerceFiles = Directory.GetFiles(baseDirectory, "ACommerce.*.dll");

			foreach (var file in acommerceFiles)
			{
				try
				{
					var assemblyName = AssemblyName.GetAssemblyName(file);

					// Check if already loaded
					var existingAssembly = loadedAssemblies
						.FirstOrDefault(a => a.GetName().Name == assemblyName.Name);

					if (existingAssembly == null)
					{
						var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
						loadedAssemblies.Add(assembly);
					}
				}
				catch
				{
					// Skip assemblies that can't be loaded
					continue;
				}
			}
		}
		catch
		{
			// If directory scanning fails, just use the initial assemblies
		}

		return loadedAssemblies.ToArray();
	}

	/// <summary>
	/// Auto-discover entities and their DTOs and register CQRS handlers
	/// </summary>
	private static void RegisterCqrsHandlersForEntities(IServiceCollection services, Assembly[] assemblies)
	{
		var entityInterface = typeof(IBaseEntity);
		var registeredPairs = new HashSet<string>();

		// First, collect all entity types and DTO types from all assemblies
		var allEntityTypes = new List<Type>();
		var allDtoTypes = new Dictionary<string, Type>();

		foreach (var assembly in assemblies)
		{
			try
			{
				var types = GetLoadableTypes(assembly);

				// Collect entities
				allEntityTypes.AddRange(types.Where(t =>
					entityInterface.IsAssignableFrom(t)
					&& t.IsClass
					&& !t.IsAbstract
					&& !t.IsGenericType
					&& !t.IsNested));

				// Collect DTOs (index by name for fast lookup)
				foreach (var dtoType in types.Where(t =>
					t.IsClass
					&& !t.IsAbstract
					&& !t.IsGenericType
					&& (t.Name.EndsWith("ResponseDto") || t.Name.EndsWith("Dto"))))
				{
					// Use the first DTO found with each name
					if (!allDtoTypes.ContainsKey(dtoType.Name))
					{
						allDtoTypes[dtoType.Name] = dtoType;
					}
				}
			}
			catch
			{
				continue;
			}
		}

		// Now match entities with their DTOs and register handlers
		foreach (var entityType in allEntityTypes)
		{
			var entityName = entityType.Name;
			var pairKey = entityType.FullName ?? entityName;

			if (registeredPairs.Contains(pairKey))
				continue;

			// Try to find matching DTO
			Type? responseDtoType = null;

			// Try exact match first: EntityNameResponseDto
			if (allDtoTypes.TryGetValue($"{entityName}ResponseDto", out var exactMatch))
			{
				responseDtoType = exactMatch;
			}
			// Try EntityNameDto
			else if (allDtoTypes.TryGetValue($"{entityName}Dto", out var dtoMatch))
			{
				responseDtoType = dtoMatch;
			}

			if (responseDtoType != null)
			{
				try
				{
					RegisterHandlersForEntity(services, entityType, responseDtoType);
					registeredPairs.Add(pairKey);
				}
				catch
				{
					// Skip entities that fail to register
				}
			}
		}
	}

	/// <summary>
	/// Get types from assembly, handling ReflectionTypeLoadException
	/// </summary>
	private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			return ex.Types.Where(t => t != null).Cast<Type>();
		}
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

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

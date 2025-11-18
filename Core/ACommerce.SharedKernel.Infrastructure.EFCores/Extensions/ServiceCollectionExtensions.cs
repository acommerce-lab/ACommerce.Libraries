using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.SharedKernel.Infrastructure.EFCore.Factories;
using ACommerce.SharedKernel.Infrastructure.EFCore.Repositories;
using System.Reflection;

namespace ACommerce.SharedKernel.Infrastructure.EFCore.Extensions;

public static class ServiceCollectionExtensions
{
	/// <summary>
	/// إضافة EF Core Infrastructure مع DbContext محدد
	/// </summary>
	public static IServiceCollection AddEfCoreInfrastructure<TDbContext>(
		this IServiceCollection services)
		where TDbContext : DbContext
	{
		// تسجيل DbContext كـ Generic
		services.AddScoped<DbContext>(provider => provider.GetRequiredService<TDbContext>());

		// تسجيل المستودع العام
		services.AddScoped(typeof(IBaseAsyncRepository<>), typeof(BaseAsyncRepository<>));

		// تسجيل Repository Factory
		services.AddScoped<IRepositoryFactory, RepositoryFactory>();

		return services;
	}

	/// <summary>
	/// إضافة EF Core Infrastructure مع CQRS
	/// </summary>
	public static IServiceCollection AddEfCoreWithCQRS<TDbContext>(
		this IServiceCollection services,
		params Assembly[] assemblies)
		where TDbContext : DbContext
	{
		// إضافة Infrastructure
		services.AddEfCoreInfrastructure<TDbContext>();

		// إضافة CQRS (من المكتبة الأخرى)
		// يتم استدعاء AddSharedKernelCQRS من ACommerce.SharedKernel.CQRS
		// services.AddSharedKernelCQRS(assemblies);

		return services;
	}
}

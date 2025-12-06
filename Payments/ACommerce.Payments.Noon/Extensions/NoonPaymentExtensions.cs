using ACommerce.Payments.Abstractions.Contracts;
using ACommerce.Payments.Noon.Models;
using ACommerce.Payments.Noon.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Payments.Noon.Extensions;

/// <summary>
/// امتدادات لتسجيل مزود دفع نون
/// </summary>
public static class NoonPaymentExtensions
{
	/// <summary>
	/// إضافة مزود دفع نون
	/// </summary>
	/// <param name="services">مجموعة الخدمات</param>
	/// <param name="configuration">التكوين</param>
	/// <returns>مجموعة الخدمات</returns>
	public static IServiceCollection AddNoonPayments(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services.Configure<NoonOptions>(configuration.GetSection("Payments:Noon"));
		services.AddHttpClient("NoonPayments");
		services.AddScoped<IPaymentProvider, NoonPaymentProvider>();
		services.AddScoped<NoonPaymentProvider>();

		return services;
	}

	/// <summary>
	/// إضافة مزود دفع نون مع إعدادات مخصصة
	/// </summary>
	/// <param name="services">مجموعة الخدمات</param>
	/// <param name="configureOptions">دالة تكوين الإعدادات</param>
	/// <returns>مجموعة الخدمات</returns>
	public static IServiceCollection AddNoonPayments(
		this IServiceCollection services,
		Action<NoonOptions> configureOptions)
	{
		services.Configure(configureOptions);
		services.AddHttpClient("NoonPayments");
		services.AddScoped<IPaymentProvider, NoonPaymentProvider>();
		services.AddScoped<NoonPaymentProvider>();

		return services;
	}

	/// <summary>
	/// إضافة مزود دفع نون باسم محدد (للتعدد)
	/// </summary>
	/// <param name="services">مجموعة الخدمات</param>
	/// <param name="name">اسم المزود</param>
	/// <param name="configureOptions">دالة تكوين الإعدادات</param>
	/// <returns>مجموعة الخدمات</returns>
	public static IServiceCollection AddNoonPayments(
		this IServiceCollection services,
		string name,
		Action<NoonOptions> configureOptions)
	{
		services.Configure(name, configureOptions);
		services.AddHttpClient($"NoonPayments_{name}");
		services.AddKeyedScoped<IPaymentProvider, NoonPaymentProvider>(name);

		return services;
	}
}

using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Channels.InApp.Options;
using ACommerce.Notifications.Channels.InApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Notifications.Channels.InApp.Extensions;

public static class ServiceCollectionExtensions
{
	/// <summary>
	/// ????? ???? ????????? ???? ???????
	/// </summary>
	public static IServiceCollection AddInAppNotifications(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		// ????? ????????
		var options = new InAppNotificationOptions();
		configuration.GetSection(InAppNotificationOptions.SectionName)
			.Bind(options);
		services.AddSingleton(options);

		// ????? ??????
		services.AddScoped<INotificationChannel, InAppNotificationChannel>();

		// ????? ?????? ????????
		services.AddScoped<InAppNotificationService>();

		return services;
	}

	/// <summary>
	/// ????? ???? ????????? ???? ??????? ?? ????? ????
	/// </summary>
	public static IServiceCollection AddInAppNotifications(
		this IServiceCollection services,
		Action<InAppNotificationOptions>? configure = null)
	{
		var options = new InAppNotificationOptions();
		configure?.Invoke(options);
		services.AddSingleton(options);

		services.AddScoped<INotificationChannel, InAppNotificationChannel>();
		services.AddScoped<InAppNotificationService>();

		return services;
	}
}


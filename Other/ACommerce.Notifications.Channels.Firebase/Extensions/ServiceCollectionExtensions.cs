using ACommerce.Notifications.Abstractions;
using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Channels.Firebase.Options;
using ACommerce.Notifications.Channels.Firebase.Services;
using ACommerce.Notifications.Channels.Firebase.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Notifications.Channels.Firebase;

public static class ServiceCollectionExtensions
{
	/// <summary>
	/// ????? ???? Firebase Cloud Messaging
	/// </summary>
	public static IServiceCollection AddFirebaseNotifications(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		// ????? ????????
		var options = new FirebaseOptions
		{
			ServiceAccountKeyPath = configuration[$"{FirebaseOptions.SectionName}:ServiceAccountKeyPath"],
			ServiceAccountKeyJson = configuration[$"{FirebaseOptions.SectionName}:ServiceAccountKeyJson"],
			ProjectId = configuration[$"{FirebaseOptions.SectionName}:ProjectId"]
		};

		if (string.IsNullOrEmpty(options.ServiceAccountKeyPath) &&
			string.IsNullOrEmpty(options.ServiceAccountKeyJson))
		{
			throw new InvalidOperationException(
				"Either ServiceAccountKeyPath or ServiceAccountKeyJson must be configured");
		}

		services.AddSingleton(options);

		// ????? ???????
		services.AddSingleton<FirebaseMessagingService>();
		services.AddSingleton<IFirebaseTokenStore, InMemoryFirebaseTokenStore>();
		services.AddScoped<INotificationChannel, FirebaseNotificationChannel>();

		return services;
	}

	/// <summary>
	/// ????? ???? Firebase ?? ????? ????
	/// </summary>
	public static IServiceCollection AddFirebaseNotifications(
		this IServiceCollection services,
		Action<FirebaseOptions> configure)
	{
		var options = new FirebaseOptions();
		configure(options);

		if (string.IsNullOrEmpty(options.ServiceAccountKeyPath) &&
			string.IsNullOrEmpty(options.ServiceAccountKeyJson))
		{
			throw new InvalidOperationException(
				"Either ServiceAccountKeyPath or ServiceAccountKeyJson must be provided");
		}

		services.AddSingleton(options);
		services.AddSingleton<FirebaseMessagingService>();
		services.AddSingleton<IFirebaseTokenStore, InMemoryFirebaseTokenStore>();
		services.AddScoped<INotificationChannel, FirebaseNotificationChannel>();

		return services;
	}

	/// <summary>
	/// ????? Firebase ?? Custom Token Store
	/// </summary>
	public static IServiceCollection AddFirebaseNotifications<TTokenStore>(
		this IServiceCollection services,
		IConfiguration configuration)
		where TTokenStore : class, IFirebaseTokenStore
	{
		services.AddFirebaseNotifications(configuration);

		// ??????? Token Store
		services.AddSingleton<IFirebaseTokenStore, TTokenStore>();

		return services;
	}
}


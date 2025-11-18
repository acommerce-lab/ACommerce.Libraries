using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Core.Options;
using ACommerce.Notifications.Core.Publishers;
using ACommerce.Notifications.Core.Services;
using ACommerce.Notifications.Core.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ACommerce.Notifications.Core.Extensions;

public static class ServiceCollectionExtensions
{
	/// <summary>
	/// ????? ???? ????????? ????????
	/// </summary>
	public static IServiceCollection AddNotificationCore(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		// ? ?????: ??????? Bind ????? ?? ??????? ???????
		services.AddOptions<NotificationOptions>()
			.Bind(configuration.GetSection(NotificationOptions.SectionName))
			.ValidateDataAnnotations()
			.ValidateOnStart();

		// ? ????? Singleton ?????? ???????
		services.AddSingleton(sp =>
			sp.GetRequiredService<IOptions<NotificationOptions>>().Value);

		// ?????? ????????
		services.AddScoped<INotificationService, NotificationService>();

		return services;
	}

	/// <summary>
	/// ????? ?????? InMemory (???????)
	/// </summary>
	public static IServiceCollection AddInMemoryNotificationPublisher(
		this IServiceCollection services)
	{
		services.AddScoped<INotificationPublisher, InMemoryNotificationPublisher>();
		return services;
	}

	/// <summary>
	/// ????? ?????? HTTP (??????? ?? Notification Service ??????)
	/// </summary>
	public static IServiceCollection AddHttpNotificationPublisher(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		// ? ????? ???????? ?? Configuration
		var serviceUrl = configuration[$"{HttpNotificationPublisherOptions.SectionName}:ServiceUrl"];

		if (string.IsNullOrEmpty(serviceUrl))
		{
			throw new InvalidOperationException(
				$"Configuration '{HttpNotificationPublisherOptions.SectionName}:ServiceUrl' is required");
		}

		var options = new HttpNotificationPublisherOptions
		{
			ServiceUrl = serviceUrl,
			PublishEndpoint = configuration[$"{HttpNotificationPublisherOptions.SectionName}:PublishEndpoint"]
				?? "/api/notifications/publish",
			BatchPublishEndpoint = configuration[$"{HttpNotificationPublisherOptions.SectionName}:BatchPublishEndpoint"]
				?? "/api/notifications/publish-batch"
		};

		services.AddSingleton(options);

		// ? ?????: AddHttpClient ????? ????
		services.AddHttpClient<INotificationPublisher, HttpNotificationPublisher>(client =>
		{
			client.BaseAddress = new Uri(options.ServiceUrl);
			client.Timeout = options.Timeout;
		});

		return services;
	}

	/// <summary>
	/// ????? ?????? HTTP ?? ????? ????
	/// </summary>
	public static IServiceCollection AddHttpNotificationPublisher(
		this IServiceCollection services,
		Action<HttpNotificationPublisherOptions> configure)
	{
		var options = new HttpNotificationPublisherOptions { ServiceUrl = string.Empty };
		configure(options);

		if (string.IsNullOrEmpty(options.ServiceUrl))
		{
			throw new InvalidOperationException("ServiceUrl is required");
		}

		services.AddSingleton(options);

		services.AddHttpClient<INotificationPublisher, HttpNotificationPublisher>(client =>
		{
			client.BaseAddress = new Uri(options.ServiceUrl);
			client.Timeout = options.Timeout;
		});

		return services;
	}

	/// <summary>
	/// ????? Retry Worker
	/// </summary>
	public static IServiceCollection AddNotificationRetryWorker(
		this IServiceCollection services)
	{
		services.AddHostedService<NotificationRetryWorker>();
		return services;
	}
}


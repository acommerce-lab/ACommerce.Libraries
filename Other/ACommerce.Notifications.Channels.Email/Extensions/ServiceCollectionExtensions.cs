using ACommerce.Notifications.Abstractions.Contracts;
using ACommerce.Notifications.Channels.Email.Options;
using ACommerce.Notifications.Channels.Email.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Notifications.Channels.Email.Extensions;

public static class ServiceCollectionExtensions
{
	/// <summary>
	/// ????? ???? ?????? ??????????
	/// </summary>
	public static IServiceCollection AddEmailNotifications(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		// ????? ????????
		var options = new EmailOptions
		{
			Host = configuration[$"{EmailOptions.SectionName}:Host"]!,
			Port = int.Parse(configuration[$"{EmailOptions.SectionName}:Port"] ?? "587"),
			Username = configuration[$"{EmailOptions.SectionName}:Username"]!,
			Password = configuration[$"{EmailOptions.SectionName}:Password"]!,
			FromAddress = configuration[$"{EmailOptions.SectionName}:FromAddress"]!,
			FromName = configuration[$"{EmailOptions.SectionName}:FromName"] ?? "ACommerce",
			EnableSsl = bool.Parse(configuration[$"{EmailOptions.SectionName}:EnableSsl"] ?? "true")
		};

		services.AddSingleton(options);

		// ????? ???????
		services.AddSingleton<EmailTemplateService>();
		services.AddScoped<INotificationChannel, EmailNotificationChannel>();

		return services;
	}

	/// <summary>
	/// ????? ???? ?????? ?????????? ?? ????? ????
	/// </summary>
	public static IServiceCollection AddEmailNotifications(
		this IServiceCollection services,
		Action<EmailOptions> configure)
	{
		var options = new EmailOptions
		{
			Username = string.Empty,
			Password = string.Empty,
			FromAddress = string.Empty
		};

		configure(options);

		if (string.IsNullOrEmpty(options.Username) ||
			string.IsNullOrEmpty(options.Password) ||
			string.IsNullOrEmpty(options.FromAddress))
		{
			throw new InvalidOperationException(
				"Username, Password, and FromAddress are required for Email notifications");
		}

		services.AddSingleton(options);
		services.AddSingleton<EmailTemplateService>();
		services.AddScoped<INotificationChannel, EmailNotificationChannel>();

		return services;
	}
}


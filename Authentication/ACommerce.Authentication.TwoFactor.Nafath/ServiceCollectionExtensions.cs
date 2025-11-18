// ACommerce.Authentication.TwoFactor.Nafath/ServiceCollectionExtensions.cs
using ACommerce.Authentication.Abstractions;
using ACommerce.Authentication.TwoFactor.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ACommerce.Authentication.TwoFactor.Nafath;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddNafathTwoFactor(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		// Options
		services.AddOptions<NafathOptions>()
			.Bind(configuration.GetSection(NafathOptions.SectionName))
			.ValidateDataAnnotations()
			.ValidateOnStart();

		// HttpClient
		services.AddHttpClient(NafathOptions.HttpClientName, (sp, client) =>
		{
			var options = sp.GetRequiredService<IOptions<NafathOptions>>().Value;
			client.BaseAddress = new Uri(options.BaseUrl);
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		});

		// Session Store (In-Memory ??????? - ?????? ?? Redis ?? ???????)
		services.AddSingleton<ITwoFactorSessionStore, InMemoryTwoFactorSessionStore>();

		// Provider
		services.AddScoped<ITwoFactorAuthenticationProvider, NafathAuthenticationProvider>();

		return services;
	}

	public static IServiceCollection AddNafathTwoFactor(
		this IServiceCollection services,
		Action<NafathOptions> configure)
	{
		// Options
		services.AddOptions<NafathOptions>()
			.Configure(configure)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		// HttpClient
		services.AddHttpClient(NafathOptions.HttpClientName, (sp, client) =>
		{
			var options = sp.GetRequiredService<IOptions<NafathOptions>>().Value;
			client.BaseAddress = new Uri(options.BaseUrl);
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		});

		// Session Store
		services.AddSingleton<ITwoFactorSessionStore, InMemoryTwoFactorSessionStore>();

		// Provider
		services.AddScoped<ITwoFactorAuthenticationProvider, NafathAuthenticationProvider>();

		return services;
	}
}


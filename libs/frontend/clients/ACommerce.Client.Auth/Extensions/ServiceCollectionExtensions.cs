using ACommerce.Client.Core.Extensions;
using ACommerce.Client.Core.Interceptors;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Client.Auth.Extensions;

/// <summary>
/// Extensions لتسجيل Authentication Client
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// إضافة Authentication Client
	/// </summary>
	public static IServiceCollection AddAuthClient(
		this IServiceCollection services,
		string registryUrl)
	{
		// Token Manager
		services.AddSingleton<TokenManager>();

		// ACommerce Client مع Authentication
		services.AddACommerceClient(registryUrl, options =>
		{
			options.EnableAuthentication = true;
			options.TokenProvider = sp => sp.GetRequiredService<TokenManager>();
		});

		// Auth Client
		services.AddScoped<AuthClient>();

		return services;
	}
}

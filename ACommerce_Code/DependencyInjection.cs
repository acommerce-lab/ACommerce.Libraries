using ACommerce.Auth.Core.Application.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Auth.Core.Dependencies;

public static class DependencyInjection
{
	public static IServiceCollection AddACommerceAuthentication(this IServiceCollection services, IConfiguration configuration)
	{
		var provider = configuration["Auth:Provider"];
		if (string.IsNullOrWhiteSpace(provider))
			throw new InvalidOperationException("Auth:Provider not configured.");

		// ????? ?? ?? ??? ????? IAuthConfigurator ?? ???? ????????? ????????
		var configuratorType = AppDomain.CurrentDomain
			.GetAssemblies()
			.SelectMany(a => a.GetTypes())
			.FirstOrDefault(t =>
				typeof(IAuthConfigurator).IsAssignableFrom(t) &&
				!t.IsInterface && !t.IsAbstract &&
				t.Name.StartsWith(provider, StringComparison.OrdinalIgnoreCase))
			?? throw new InvalidOperationException($"No dependency registrar found for provider '{provider}'.");

		// ????? ???? ?? ????? ???? ?? ?????? ???? ???????? ????? ???????
		var configurator = (IAuthConfigurator)Activator.CreateInstance(configuratorType)!;

		configurator.ConfigureAuthentication(services, configuration);
		configurator.ConfigureAuthorization(services, configuration);
		configurator.Register(services, configuration);

		return services;
	}
}


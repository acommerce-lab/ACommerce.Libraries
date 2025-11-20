using ACommerce.Auth.Authentica.Domain.DTOs;
using ACommerce.Auth.Core.Application.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Auth.Authentica;

public class NafathAuthConfigurator : IAuthConfigurator
{
	public void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
	{
		var opts = new NafathOptions();
		configuration.GetSection("Auth:Nafath").Bind(opts);

		services.AddHttpClient("Nafath", client =>
		{
			client.BaseAddress = new Uri(opts.BaseUrl ?? throw new InvalidOperationException("Nafath:BaseUrl required."));
		});

		services.AddScoped<IAuthService, NafathAuthService>();
	}

	public void ConfigureAuthorization(IServiceCollection services, IConfiguration configuration)
	{
		services.AddAuthorization();
	}

	public void Register(IServiceCollection services, IConfiguration configuration)
	{
		var options = configuration.GetSection("Auth:Nafath");
		services.Configure<NafathOptions>(options);
		services.AddHttpClient<IAuthService, NafathAuthService>(client =>
		{
			client.BaseAddress = new Uri(options["BaseUrl"] ?? "");
		});
	}
}


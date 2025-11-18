using ACommerce.Auth.Core.Application.Contracts;
using ACommerce.Auth.OpenIddict.Domain.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Auth.OpenIddict;

public class OpenIddictAuthConfigurator : IAuthConfigurator
{
	public void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
	{
		var opts = new OpenIddictOptions();
		configuration.GetSection("Auth:OpenIddict").Bind(opts);

		//services.AddOpenIddict()
		//	.AddCore(o => o.UseEntityFrameworkCore().UseDbContext<ACommerceAuthdbContext>())
		//	.AddServer(o =>
		//	{
		//		o.AllowPasswordFlow();
		//		o.AllowRefreshTokenFlow();
		//		o.AcceptAnonymousClients();

		//		o.AddSigningKey(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.SigningKey ?? "temp-key")));
		//		o.AddEncryptionKey(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.EncryptionKey ?? "temp-enc")));

		//		o.SetTokenEndpointUris(opts.TokenEndpoint ?? "/api/connect/token");
		//		o.UseAspNetCore().EnableTokenEndpointPassthrough();
		//	})
		//	.AddValidation(o => o.UseLocalServer().UseAspNetCore());
	}

	public void ConfigureAuthorization(IServiceCollection services, IConfiguration configuration)
	{
		services.AddAuthorizationBuilder()
			.AddPolicy("ValidToken", policy =>
				policy.RequireAuthenticatedUser());
	}

	public void Register(IServiceCollection services, IConfiguration configuration)
	{
		var options = configuration.GetSection("Auth:OpenIddict");
		services.Configure<OpenIddictOptions>(options);
		services.AddScoped<IAuthService, OpenIddictAuthService>();
	}
}


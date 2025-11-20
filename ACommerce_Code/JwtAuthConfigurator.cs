using ACommerce.Auth.Core.Application.Contracts;
using ACommerce.Auth.JWT.Domain.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ACommerce.Auth.JWT;

public class JwtAuthConfigurator : IAuthConfigurator
{
	public void ConfigureAuthentication(
		IServiceCollection services,
		IConfiguration configuration)
	{
		var jwtOptions = new JwtOptions();
		configuration.GetSection("Auth:Jwt").Bind(jwtOptions);

		if (string.IsNullOrEmpty(jwtOptions.Key))
			throw new InvalidOperationException("Jwt Key is missing.");

		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.SaveToken = true;
				options.RequireHttpsMetadata = false;
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = jwtOptions.Issuer,
					ValidAudience = jwtOptions.Audience,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
				};
			});
	}

	public void ConfigureAuthorization(IServiceCollection services, IConfiguration configuration)
	{
		var audience = configuration["Auth:Jwt:Audience"];
		services.AddAuthorizationBuilder()
			.AddPolicy("ValidToken", policy =>
			{
				policy.RequireAuthenticatedUser();
				if (!string.IsNullOrEmpty(audience))
					policy.RequireClaim("aud", audience);
			});
	}

	public void Register(IServiceCollection services, IConfiguration configuration)
	{
		var options = configuration.GetSection("Auth:Jwt");
		services.Configure<JwtOptions>(options);
		services.AddScoped<IAuthService, JwtAuthService>();
	}
}


using ACommerce.Authentication.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ACommerce.Authentication.JWT;

/// <summary>
/// Extension methods for configuring JWT authentication in ASP.NET Core
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Adds JWT authentication provider and configures ASP.NET Core JWT Bearer authentication
	/// </summary>
	public static IServiceCollection AddJwtAuthentication(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		// Bind and validate JWT options
		var jwtSection = configuration.GetSection(JwtOptions.SectionName);

		services.AddOptions<JwtOptions>()
			.Bind(jwtSection)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		// Get JWT options for middleware configuration
		var jwtOptions = jwtSection.Get<JwtOptions>();

		if (jwtOptions == null)
			throw new InvalidOperationException(
				$"JWT configuration section '{JwtOptions.SectionName}' not found in appsettings.json");

		// Register authentication provider
		services.AddSingleton<IAuthenticationProvider, JwtAuthenticationProvider>();

		// Configure ASP.NET Core Authentication with JWT Bearer
		services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
		})
		.AddJwtBearer(options =>
		{
			var key = Encoding.UTF8.GetBytes(jwtOptions.SecretKey);

			options.SaveToken = true;
			options.RequireHttpsMetadata = true; // Change to false for development
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateIssuer = true,
				ValidIssuer = jwtOptions.Issuer,
				ValidateAudience = true,
				ValidAudience = jwtOptions.Audience,
				ValidateLifetime = true,
				ClockSkew = TimeSpan.Zero, // No tolerance for expired tokens
				RequireExpirationTime = true,
				RequireSignedTokens = true
			};

			// Add custom event handlers
			options.Events = new JwtBearerEvents
			{
				// استخراج التوكن من query string لـ SignalR connections
				OnMessageReceived = context =>
				{
					var accessToken = context.Request.Query["access_token"];

					// إذا كان الطلب لـ SignalR hub
					var path = context.HttpContext.Request.Path;
					if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
					{
						context.Token = accessToken;
					}

					return Task.CompletedTask;
				},
				OnAuthenticationFailed = context =>
				{
					if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
					{
						context.Response.Headers.Add("Token-Expired", "true");
					}
					return Task.CompletedTask;
				},
				OnChallenge = context =>
				{
					context.HandleResponse();
					context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					context.Response.ContentType = "application/json";

					var result = System.Text.Json.JsonSerializer.Serialize(new
					{
						error = "unauthorized",
						message = "You are not authorized to access this resource"
					});

					return context.Response.WriteAsync(result);
				},
				OnForbidden = context =>
				{
					context.Response.StatusCode = StatusCodes.Status403Forbidden;
					context.Response.ContentType = "application/json";

					var result = System.Text.Json.JsonSerializer.Serialize(new
					{
						error = "forbidden",
						message = "You do not have permission to access this resource"
					});

					return context.Response.WriteAsync(result);
				}
			};
		});

		// Add Authorization services
		services.AddAuthorization();

		return services;
	}

	/// <summary>
	/// Adds JWT authentication provider with manual configuration
	/// </summary>
	public static IServiceCollection AddJwtAuthentication(
		this IServiceCollection services,
		Action<JwtOptions> configure)
	{
		// Configure and validate options
		services.AddOptions<JwtOptions>()
			.Configure(configure)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		// Get configured options
		var jwtOptions = new JwtOptions();
		configure(jwtOptions);

		// Register authentication provider
		services.AddSingleton<IAuthenticationProvider, JwtAuthenticationProvider>();

		// Configure ASP.NET Core Authentication with JWT Bearer
		services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
		})
		.AddJwtBearer(options =>
		{
			var key = Encoding.UTF8.GetBytes(jwtOptions.SecretKey);

			options.SaveToken = true;
			options.RequireHttpsMetadata = false; // Manual config often used in dev
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateIssuer = true,
				ValidIssuer = jwtOptions.Issuer,
				ValidateAudience = true,
				ValidAudience = jwtOptions.Audience,
				ValidateLifetime = true,
				ClockSkew = TimeSpan.Zero,
				RequireExpirationTime = true,
				RequireSignedTokens = true
			};
		});

		// Add Authorization services
		services.AddAuthorization();

		return services;
	}

	/// <summary>
	/// Adds only the JWT authentication provider without configuring ASP.NET Core middleware
	/// (useful for client applications or custom scenarios)
	/// </summary>
	public static IServiceCollection AddJwtAuthenticationProvider(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services.AddOptions<JwtOptions>()
			.Bind(configuration.GetSection(JwtOptions.SectionName))
			.ValidateDataAnnotations()
			.ValidateOnStart();

		services.AddSingleton<IAuthenticationProvider, JwtAuthenticationProvider>();

		return services;
	}

	/// <summary>
	/// Adds only the JWT authentication provider with manual configuration
	/// </summary>
	public static IServiceCollection AddJwtAuthenticationProvider(
		this IServiceCollection services,
		Action<JwtOptions> configure)
	{
		services.AddOptions<JwtOptions>()
			.Configure(configure)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		services.AddSingleton<IAuthenticationProvider, JwtAuthenticationProvider>();

		return services;
	}
}

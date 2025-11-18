// Extensions/ServiceCollectionExtensions.cs
using ACommerce.Authentication.AspNetCore.Validators.Users;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Authentication.AspNetCore.Extensions;

/// <summary>
/// Extensions ?????? ????? Authentication ?? ASP.NET Core
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// ????? Controllers ? Validators ????????
	/// </summary>
	public static IServiceCollection AddACommerceAuthenticationControllers(
		this IServiceCollection services)
	{
		// ????? Controllers
		services.AddControllers()
			.AddApplicationPart(typeof(Controllers.UsersController).Assembly);

		// ????? FluentValidation Validators
		services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();

		return services;
	}
}


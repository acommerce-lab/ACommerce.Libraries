// Extensions/ServiceCollectionExtensions.cs
using ACommerce.Files.AspNetCore.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Files.AspNetCore.Extensions;

public static class ServiceCollectionExtensions
{
	/// <summary>
	/// ????? Controllers ? Validators ???????
	/// </summary>
	public static IServiceCollection AddFileControllers(
		this IServiceCollection services)
	{
		// Controllers
		services.AddControllers()
			.AddApplicationPart(typeof(Controllers.FilesController).Assembly);

		// Validators
		services.AddValidatorsFromAssemblyContaining<UploadFileDtoValidator>();

		return services;
	}
}


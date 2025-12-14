// Extensions/ServiceCollectionExtensions.cs
using ACommerce.Files.Abstractions.Providers;
using ACommerce.Files.ImageProcessing.Configuration;
using ACommerce.Files.ImageProcessing.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Files.ImageProcessing.Extensions;

public static class ServiceCollectionExtensions
{
	/// <summary>
	/// ????? ?????? ?????
	/// </summary>
	public static IServiceCollection AddImageProcessing(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		// Options
		services.AddOptions<ImageProcessorSettings>()
			.Bind(configuration.GetSection(ImageProcessorSettings.SectionName))
			.ValidateDataAnnotations()
			.ValidateOnStart();

		// Image Processor
		services.AddSingleton<IImageProcessor, ImageSharpProcessor>();

		return services;
	}

	/// <summary>
	/// ????? ?????? ????? ?????? ????
	/// </summary>
	public static IServiceCollection AddImageProcessing(
		this IServiceCollection services,
		Action<ImageProcessorSettings> configure)
	{
		// Options
		services.AddOptions<ImageProcessorSettings>()
			.Configure(configure)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		// Image Processor
		services.AddSingleton<IImageProcessor, ImageSharpProcessor>();

		return services;
	}
}


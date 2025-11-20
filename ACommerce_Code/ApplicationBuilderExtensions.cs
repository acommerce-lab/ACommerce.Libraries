using Microsoft.AspNetCore.Builder;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ACommerce.Authentication.AspNetCore.Swagger;

public static class ApplicationBuilderExtensions
{
	/// <summary>
	/// Configures Swagger UI for ACommerce Authentication
	/// </summary>
	public static IApplicationBuilder UseACommerceSwagger(
		this IApplicationBuilder app,
		Action<ACommerceSwaggerUIOptions>? configure = null)
	{
		var options = new ACommerceSwaggerUIOptions();
		configure?.Invoke(options);

		app.UseSwagger();

		app.UseSwaggerUI(c =>
		{
			c.SwaggerEndpoint(
				$"/swagger/{options.Version}/swagger.json",
				$"{options.Title} {options.Version}");

			// UI Customization
			c.RoutePrefix = options.RoutePrefix;
			c.DocumentTitle = options.DocumentTitle;
			c.DefaultModelsExpandDepth(options.DefaultModelsExpandDepth);
			c.DefaultModelExpandDepth(options.DefaultModelExpandDepth);
			c.DocExpansion(options.DocExpansion);
			c.EnableDeepLinking();
			c.DisplayRequestDuration();
			c.EnableFilter();
			c.ShowExtensions();

			// RTL Support for Arabic
			if (options.EnableRtlSupport)
			{
				c.InjectStylesheet("/swagger-ui/rtl.css");
			}

			// JWT Authorization
			if (options.EnablePersistAuthorization)
			{
				c.EnablePersistAuthorization();
			}

			// Custom CSS
			if (!string.IsNullOrEmpty(options.CustomCssPath))
			{
				c.InjectStylesheet(options.CustomCssPath);
			}
		});

		return app;
	}
}

public class ACommerceSwaggerUIOptions
{
	public string Version { get; set; } = "v1";
	public string Title { get; set; } = "ACommerce Authentication API";
	public string RoutePrefix { get; set; } = "swagger";
	public string DocumentTitle { get; set; } = "ACommerce Authentication API - Swagger UI";
	public int DefaultModelsExpandDepth { get; set; } = 1;
	public int DefaultModelExpandDepth { get; set; } = 1;
	public DocExpansion DocExpansion { get; set; } = DocExpansion.List;
	public bool EnableRtlSupport { get; set; } = true;
	public bool EnablePersistAuthorization { get; set; } = true;
	public string? CustomCssPath { get; set; }
}


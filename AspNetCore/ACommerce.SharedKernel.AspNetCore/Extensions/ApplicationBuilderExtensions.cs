using Microsoft.AspNetCore.Builder;
using ACommerce.SharedKernel.AspNetCore.Middleware;

namespace ACommerce.SharedKernel.AspNetCore.Extensions;

public static class ApplicationBuilderExtensions
{
	/// <summary>
	/// إضافة Global Exception Middleware
	/// </summary>
	public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
	{
		return app.UseMiddleware<GlobalExceptionMiddleware>();
	}
}

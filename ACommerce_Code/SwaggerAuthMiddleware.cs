using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace ACommerce.Authentication.AspNetCore.Swagger;

/// <summary>
/// Optional middleware to protect Swagger UI with basic authentication
/// </summary>
public class SwaggerAuthMiddleware
{
	private readonly RequestDelegate _next;
	private readonly string _username;
	private readonly string _password;

	public SwaggerAuthMiddleware(
		RequestDelegate next,
		string username,
		string password)
	{
		_next = next;
		_username = username;
		_password = password;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		// Check if request is for Swagger UI
		if (context.Request.Path.StartsWithSegments("/swagger"))
		{
			string? authHeader = context.Request.Headers["Authorization"];

			if (authHeader != null && authHeader.StartsWith("Basic "))
			{
				var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
				var credentials = System.Text.Encoding.UTF8
					.GetString(Convert.FromBase64String(encodedCredentials))
					.Split(':', 2);

				if (credentials.Length == 2 &&
					credentials[0] == _username &&
					credentials[1] == _password)
				{
					await _next(context);
					return;
				}
			}

			// Return 401 with WWW-Authenticate header
			context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Swagger UI\"";
			await context.Response.WriteAsync("Unauthorized");
			return;
		}

		await _next(context);
	}
}

public static class SwaggerAuthMiddlewareExtensions
{
	/// <summary>
	/// Protects Swagger UI with basic authentication
	/// </summary>
	public static IApplicationBuilder UseSwaggerBasicAuth(
		this IApplicationBuilder app,
		string username,
		string password)
	{
		return app.UseMiddleware<SwaggerAuthMiddleware>(username, password);
	}
}


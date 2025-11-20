using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ACommerce.SharedKernel.Abstractions.Exceptions;
using System.Text.Json;

namespace ACommerce.SharedKernel.AspNetCore.Middleware;

/// <summary>
/// Middleware للتعامل مع الأخطاء بشكل مركزي
/// </summary>
public class GlobalExceptionMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<GlobalExceptionMiddleware> _logger;

	public GlobalExceptionMiddleware(
		RequestDelegate next,
		ILogger<GlobalExceptionMiddleware> logger)
	{
		_next = next ?? throw new ArgumentNullException(nameof(next));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An unhandled exception occurred");
			await HandleExceptionAsync(context, ex);
		}
	}

	private async Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		context.Response.ContentType = "application/json";
		var statusCode = 500;
		object? responseBody = null;

		switch (exception)
		{
			case FluentValidation.ValidationException validationException:
				statusCode = 400;
				var errors = validationException.Errors.Select(e => new
				{
					field = e.PropertyName,
					message = e.ErrorMessage
				});
				responseBody = new { message = "Validation failed", errors };
				_logger.LogWarning("Validation failed: {@Errors}", errors);
				break;

			case ACommerce.SharedKernel.Abstractions.Exceptions.ValidationException customValidationException:
				statusCode = 400;
				responseBody = new
				{
					message = "Validation failed",
					errors = customValidationException.Errors.Select(kvp => new
					{
						field = kvp.Key,
						message = kvp.Value
					})
				};
				_logger.LogWarning("Custom validation failed: {@Errors}", customValidationException.Errors);
				break;

			case EntityNotFoundException notFoundException:
				statusCode = 404;
				responseBody = new
				{
					message = notFoundException.Message,
					errorCode = notFoundException.ErrorCode
				};
				_logger.LogWarning("Entity not found: {Message}", notFoundException.Message);
				break;

			case KeyNotFoundException:
				statusCode = 404;
				responseBody = new { message = "The requested resource was not found" };
				_logger.LogWarning("Resource not found: {Message}", exception.Message);
				break;

			case UnauthorizedException unauthorizedException:
				statusCode = 401;
				responseBody = new
				{
					message = unauthorizedException.Message,
					errorCode = unauthorizedException.ErrorCode
				};
				_logger.LogWarning("Unauthorized access: {Message}", unauthorizedException.Message);
				break;

			case ConcurrencyException concurrencyException:
				statusCode = 409;
				responseBody = new
				{
					message = concurrencyException.Message,
					errorCode = concurrencyException.ErrorCode
				};
				_logger.LogWarning("Concurrency error: {Message}", concurrencyException.Message);
				break;

			case DbUpdateConcurrencyException:
				statusCode = 409;
				responseBody = new
				{
					message = "The data has been modified by another user. Please refresh and try again.",
					errorCode = "CONCURRENCY_ERROR"
				};
				_logger.LogWarning("Database concurrency error");
				break;

			case DomainException domainException:
				statusCode = 400;
				responseBody = new
				{
					message = domainException.Message,
					errorCode = domainException.ErrorCode
				};
				_logger.LogWarning("Domain exception: {Message}", domainException.Message);
				break;

			default:
				statusCode = 500;
				responseBody = new
				{
					message = "An internal server error occurred",
					detail = exception.Message
				};
				_logger.LogError(exception, "Unhandled exception occurred");
				break;
		}

		context.Response.StatusCode = statusCode;
		var jsonResponse = JsonSerializer.Serialize(responseBody, new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		});

		await context.Response.WriteAsync(jsonResponse);
	}
}

using System.Net;
using System.Text.Json;
using Serilog;

namespace Ashare.Api.Middleware;

/// <summary>
/// Middleware لمعالجة جميع الأخطاء بشكل شامل
/// يضمن عدم انهيار الباك اند مهما حصل
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException)
        {
            // طلب ملغي - لا نسجله كخطأ
            _logger.LogDebug("Request cancelled: {Path}", context.Request.Path);
            context.Response.StatusCode = 499; // Client Closed Request
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // تسجيل الخطأ
        _logger.LogError(exception,
            "❌ Unhandled exception on {Method} {Path}",
            context.Request.Method,
            context.Request.Path);

        // تحديد نوع الخطأ وكود الاستجابة
        var (statusCode, message) = exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "غير مصرح"),
            ArgumentException => (HttpStatusCode.BadRequest, "بيانات غير صالحة"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "غير موجود"),
            InvalidOperationException => (HttpStatusCode.BadRequest, "عملية غير صالحة"),
            TimeoutException => (HttpStatusCode.GatewayTimeout, "انتهت المهلة"),
            HttpRequestException => (HttpStatusCode.BadGateway, "خطأ في الاتصال الخارجي"),
            _ => (HttpStatusCode.InternalServerError, "حدث خطأ داخلي")
        };

        // إعداد الاستجابة
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ErrorResponse
        {
            Success = false,
            Message = message,
            TraceId = context.TraceIdentifier,
#if DEBUG
            // في وضع التطوير فقط نعرض تفاصيل الخطأ
            Details = exception.Message,
            StackTrace = exception.StackTrace
#endif
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsJsonAsync(response, jsonOptions);
    }
}

/// <summary>
/// نموذج استجابة الخطأ
/// </summary>
public class ErrorResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? TraceId { get; set; }
    public string? Details { get; set; }
    public string? StackTrace { get; set; }
}

/// <summary>
/// Extension method لتسهيل الاستخدام
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}

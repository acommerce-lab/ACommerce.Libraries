using Microsoft.AspNetCore.SignalR;

namespace Ashare.Api.Middleware;

/// <summary>
/// Filter لمعالجة الأخطاء في SignalR Hubs
/// يضمن عدم انهيار الـ Hub مهما حصل
/// </summary>
public class SignalRExceptionFilter : IHubFilter
{
    private readonly ILogger<SignalRExceptionFilter> _logger;

    public SignalRExceptionFilter(ILogger<SignalRExceptionFilter> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        try
        {
            return await next(invocationContext);
        }
        catch (OperationCanceledException)
        {
            // عملية ملغاة - لا نسجلها كخطأ
            _logger.LogDebug(
                "Hub method cancelled: {Hub}.{Method}",
                invocationContext.Hub.GetType().Name,
                invocationContext.HubMethodName);
            return null;
        }
        catch (HubException)
        {
            // خطأ Hub عادي - نمرره للعميل
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ Error in hub method {Hub}.{Method} for connection {ConnectionId}",
                invocationContext.Hub.GetType().Name,
                invocationContext.HubMethodName,
                invocationContext.Context.ConnectionId);

            // نرسل رسالة خطأ ودية للعميل بدلاً من إسقاط الاتصال
            throw new HubException("حدث خطأ في المعالجة. يرجى المحاولة مرة أخرى.");
        }
    }

    public Task OnConnectedAsync(
        HubLifetimeContext context,
        Func<HubLifetimeContext, Task> next)
    {
        return SafeExecuteAsync(context, next, "OnConnected");
    }

    public Task OnDisconnectedAsync(
        HubLifetimeContext context,
        Exception? exception,
        Func<HubLifetimeContext, Exception?, Task> next)
    {
        return SafeExecuteAsync(context, exception, next, "OnDisconnected");
    }

    private async Task SafeExecuteAsync(
        HubLifetimeContext context,
        Func<HubLifetimeContext, Task> next,
        string operation)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ Error in {Operation} for hub {Hub}, connection {ConnectionId}",
                operation,
                context.Hub.GetType().Name,
                context.Context.ConnectionId);
            // لا نرمي - نسمح للاتصال بالاستمرار
        }
    }

    private async Task SafeExecuteAsync(
        HubLifetimeContext context,
        Exception? exception,
        Func<HubLifetimeContext, Exception?, Task> next,
        string operation)
    {
        try
        {
            await next(context, exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ Error in {Operation} for hub {Hub}, connection {ConnectionId}",
                operation,
                context.Hub.GetType().Name,
                context.Context.ConnectionId);
            // لا نرمي - نسمح بالإغلاق النظيف
        }
    }
}

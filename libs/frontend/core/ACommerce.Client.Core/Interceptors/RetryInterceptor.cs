using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace ACommerce.Client.Core.Interceptors;

/// <summary>
/// Interceptor لإعادة المحاولة تلقائياً عند فشل الطلب
/// محسّن للتعامل مع بطء بدء الخدمات
/// </summary>
public sealed class RetryInterceptor : DelegatingHandler
{
        private readonly ILogger<RetryInterceptor> _logger;
        private readonly int _maxRetries;
        private readonly int _baseDelayMs;

        public RetryInterceptor(ILogger<RetryInterceptor> logger, int maxRetries = 3, int baseDelayMs = 500)
        {
                _logger = logger;
                _maxRetries = maxRetries;
                _baseDelayMs = baseDelayMs;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
        {
                HttpResponseMessage? response = null;
                Exception? lastException = null;

                for (int i = 0; i <= _maxRetries; i++)
                {
                        try
                        {
                                response = await base.SendAsync(request, cancellationToken);

                                if (response.IsSuccessStatusCode)
                                {
                                        return response;
                                }

                                if (IsNonRetryableStatusCode(response.StatusCode))
                                {
                                        return response;
                                }

                                if (i == _maxRetries)
                                {
                                        return response;
                                }

                                _logger.LogWarning(
                                        "Request to {Path} failed with status {Status}. Retry {Retry}/{MaxRetries}",
                                        request.RequestUri?.PathAndQuery, response.StatusCode, i + 1, _maxRetries);

                                await WaitBeforeRetry(i, cancellationToken);
                        }
                        catch (HttpRequestException ex) when (IsConnectionError(ex))
                        {
                                lastException = ex;

                                if (i == _maxRetries)
                                {
                                        _logger.LogError("API غير متاحة بعد {MaxRetries} محاولات. تأكد من تشغيل الخدمة.", _maxRetries);
                                        throw;
                                }

                                _logger.LogWarning(
                                        "الخدمة غير متاحة. انتظار قبل المحاولة {Retry}/{MaxRetries}...",
                                        i + 1, _maxRetries);

                                await WaitBeforeRetry(i, cancellationToken, isConnectionError: true);
                        }
                        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
                        {
                                lastException = ex;

                                if (i == _maxRetries)
                                {
                                        _logger.LogError("انتهت مهلة الطلب بعد {MaxRetries} محاولات", _maxRetries);
                                        throw;
                                }

                                _logger.LogWarning(
                                        "انتهت مهلة الطلب. المحاولة {Retry}/{MaxRetries}",
                                        i + 1, _maxRetries);

                                await WaitBeforeRetry(i, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                                lastException = ex;

                                if (i == _maxRetries)
                                {
                                        throw;
                                }

                                _logger.LogWarning(ex,
                                        "Request failed with exception. Retry {Retry}/{MaxRetries}",
                                        i + 1, _maxRetries);

                                await WaitBeforeRetry(i, cancellationToken);
                        }
                }

                if (lastException != null)
                {
                        throw lastException;
                }

                return response!;
        }

        private async Task WaitBeforeRetry(int attemptIndex, CancellationToken cancellationToken, bool isConnectionError = false)
        {
                int delayMs = isConnectionError 
                        ? _baseDelayMs * (attemptIndex + 1)
                        : (int)(Math.Pow(2, attemptIndex) * _baseDelayMs);
                
                delayMs = Math.Min(delayMs, 3000);
                
                await Task.Delay(delayMs, cancellationToken);
        }

        private static bool IsConnectionError(HttpRequestException ex)
        {
                return ex.InnerException is SocketException socketEx && 
                       socketEx.SocketErrorCode == SocketError.ConnectionRefused;
        }

        /// <summary>
        /// أكواد الحالة التي لا يجب إعادة المحاولة عليها
        /// لأن إعادة المحاولة لن تغير النتيجة
        /// </summary>
        private static bool IsNonRetryableStatusCode(System.Net.HttpStatusCode statusCode)
        {
                return statusCode switch
                {
                        System.Net.HttpStatusCode.Unauthorized => true,        // 401 - التوكن غير صالح
                        System.Net.HttpStatusCode.Forbidden => true,           // 403 - ممنوع
                        System.Net.HttpStatusCode.BadRequest => true,          // 400 - طلب خاطئ
                        System.Net.HttpStatusCode.NotFound => true,            // 404 - غير موجود
                        System.Net.HttpStatusCode.MethodNotAllowed => true,    // 405 - الطريقة غير مسموحة
                        System.Net.HttpStatusCode.Conflict => true,            // 409 - تعارض
                        System.Net.HttpStatusCode.Gone => true,                // 410 - محذوف
                        System.Net.HttpStatusCode.UnprocessableEntity => true, // 422 - غير قابل للمعالجة
                        _ => false
                };
        }
}

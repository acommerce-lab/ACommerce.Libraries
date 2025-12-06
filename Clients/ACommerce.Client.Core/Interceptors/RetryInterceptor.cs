using Microsoft.Extensions.Logging;

namespace ACommerce.Client.Core.Interceptors;

/// <summary>
/// Interceptor لإعادة المحاولة تلقائياً عند فشل الطلب
/// </summary>
public sealed class RetryInterceptor : DelegatingHandler
{
	private readonly ILogger<RetryInterceptor> _logger;
	private readonly int _maxRetries;

	public RetryInterceptor(ILogger<RetryInterceptor> logger, int maxRetries = 3)
	{
		_logger = logger;
		_maxRetries = maxRetries;
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

				// إذا نجح الطلب، ارجع
				if (response.IsSuccessStatusCode)
				{
					return response;
				}

				// لا تُعد المحاولة للأخطاء التي لن تتغير بإعادة المحاولة
				if (IsNonRetryableStatusCode(response.StatusCode))
				{
					return response;
				}

				// إذا كانت آخر محاولة، ارجع حتى لو فشل
				if (i == _maxRetries)
				{
					return response;
				}

				_logger.LogWarning(
					"Request failed with status {Status}. Retry {Retry}/{MaxRetries}",
					response.StatusCode, i + 1, _maxRetries);

				// انتظر قبل إعادة المحاولة (Exponential Backoff)
				var delay = TimeSpan.FromSeconds(Math.Pow(2, i));
				await Task.Delay(delay, cancellationToken);
			}
			catch (Exception ex)
			{
				lastException = ex;

				// إذا كانت آخر محاولة، اطرح الخطأ
				if (i == _maxRetries)
				{
					throw;
				}

				_logger.LogWarning(ex,
					"Request failed with exception. Retry {Retry}/{MaxRetries}",
					i + 1, _maxRetries);

				// انتظر قبل إعادة المحاولة
				var delay = TimeSpan.FromSeconds(Math.Pow(2, i));
				await Task.Delay(delay, cancellationToken);
			}
		}

		// إذا وصلنا هنا، فقد فشلت كل المحاولات
		if (lastException != null)
		{
			throw lastException;
		}

		return response!;
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

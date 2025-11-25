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
}

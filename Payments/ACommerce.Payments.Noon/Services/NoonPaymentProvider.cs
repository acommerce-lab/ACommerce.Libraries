using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ACommerce.Payments.Abstractions.Contracts;
using ACommerce.Payments.Abstractions.Enums;
using ACommerce.Payments.Abstractions.Models;
using ACommerce.Payments.Noon.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ACommerce.Payments.Noon.Services;

/// <summary>
/// مزود دفع نون - تنفيذ واجهة IPaymentProvider
/// </summary>
public class NoonPaymentProvider : IPaymentProvider
{
	private readonly NoonOptions _options;
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly ILogger<NoonPaymentProvider> _logger;
	private readonly JsonSerializerOptions _jsonOptions;

	public string ProviderName => "Noon";

	public NoonPaymentProvider(
		IOptions<NoonOptions> options,
		IHttpClientFactory httpClientFactory,
		ILogger<NoonPaymentProvider> logger)
	{
		_options = options.Value;
		_httpClientFactory = httpClientFactory;
		_logger = logger;
		_jsonOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
		};
	}

	/// <summary>
	/// إنشاء مفتاح التفويض
	/// </summary>
	private string CreateAuthorizationHeader()
	{
		var prefix = _options.UseSandbox ? "Key_Test" : "Key_Live";
		return $"{prefix} {_options.AuthorizationKey}";
	}

	/// <summary>
	/// إنشاء عميل HTTP مع الرؤوس المطلوبة
	/// </summary>
	private HttpClient CreateHttpClient()
	{
		var client = _httpClientFactory.CreateClient("NoonPayments");
		client.BaseAddress = new Uri(_options.ApiUrl);
		client.DefaultRequestHeaders.Clear();
		client.DefaultRequestHeaders.Add("Authorization", CreateAuthorizationHeader());
		client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
		return client;
	}

	/// <inheritdoc />
	public async Task<PaymentResult> CreatePaymentAsync(
		PaymentRequest request,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Creating Noon payment for order {OrderId}, amount {Amount} {Currency}",
				request.OrderId, request.Amount, request.Currency);

			var noonRequest = new NoonOrderRequest
			{
				ApiOperation = "INITIATE",
				Order = new NoonOrderInfo
				{
					Reference = request.OrderId,
					Amount = request.Amount,
					Currency = request.Currency ?? _options.DefaultCurrency,
					Name = request.Description ?? $"Order {request.OrderId}",
					Category = _options.DefaultOrderCategory,
					Channel = _options.DefaultChannel
				},
				Configuration = new NoonConfiguration
				{
					ReturnUrl = request.CallbackUrl ?? _options.ReturnUrl ?? throw new InvalidOperationException("Return URL is required"),
					Locale = "ar",
					PaymentAction = "SALE"
				}
			};

			// إضافة معلومات العميل إذا متوفرة
			var hasEmail = request.Metadata.TryGetValue("customerEmail", out var email);
			var hasPhone = request.Metadata.TryGetValue("customerPhone", out var phone);

			if (hasEmail || hasPhone)
			{
				noonRequest.Billing = new NoonBillingInfo
				{
					Contact = new NoonContactInfo
					{
						Email = email,
						Phone = phone,
						FirstName = request.Metadata.GetValueOrDefault("customerFirstName"),
						LastName = request.Metadata.GetValueOrDefault("customerLastName")
					}
				};
			}

			var client = CreateHttpClient();
			var json = JsonSerializer.Serialize(noonRequest, _jsonOptions);

			_logger.LogDebug("Noon request: {Request}", json);

			var content = new StringContent(json, Encoding.UTF8, "application/json");
			var response = await client.PostAsync("/order", content, cancellationToken);

			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
			_logger.LogDebug("Noon response: {Response}", responseContent);

			var noonResponse = JsonSerializer.Deserialize<NoonApiResponse>(responseContent, _jsonOptions);

			if (noonResponse == null)
			{
				return CreateFailedResult("فشل في تحليل استجابة نون");
			}

			if (noonResponse.ResultCode == NoonResultCode.Success)
			{
				var orderId = noonResponse.Result?.Order?.Id ?? string.Empty;
				var checkoutUrl = noonResponse.Result?.CheckoutData?.PostUrl;

				_logger.LogInformation("Noon payment created successfully. OrderId: {OrderId}", orderId);

				return new PaymentResult
				{
					Success = true,
					TransactionId = orderId,
					Status = PaymentStatus.Pending,
					PaymentUrl = checkoutUrl,
					Metadata = new Dictionary<string, string>
					{
						["noonOrderId"] = orderId,
						["orderReference"] = request.OrderId
					}
				};
			}

			_logger.LogWarning("Noon payment creation failed. Code: {Code}, Message: {Message}",
				noonResponse.ResultCode, noonResponse.Message);

			return CreateFailedResult(noonResponse.Message ?? "فشل في إنشاء عملية الدفع");
		}
		catch (HttpRequestException ex)
		{
			_logger.LogError(ex, "HTTP error creating Noon payment for order {OrderId}", request.OrderId);
			return CreateFailedResult($"خطأ في الاتصال: {ex.Message}");
		}
		catch (TaskCanceledException ex) when (ex.CancellationToken != cancellationToken)
		{
			_logger.LogError(ex, "Timeout creating Noon payment for order {OrderId}", request.OrderId);
			return CreateFailedResult("انتهت مهلة الاتصال");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating Noon payment for order {OrderId}", request.OrderId);
			return CreateFailedResult(ex.Message);
		}
	}

	/// <inheritdoc />
	public async Task<PaymentResult> GetPaymentStatusAsync(
		string transactionId,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Getting Noon payment status for {TransactionId}", transactionId);

			var client = CreateHttpClient();
			var response = await client.GetAsync($"/order/{transactionId}", cancellationToken);
			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

			_logger.LogDebug("Noon status response: {Response}", responseContent);

			var noonResponse = JsonSerializer.Deserialize<NoonApiResponse>(responseContent, _jsonOptions);

			if (noonResponse == null)
			{
				return CreateFailedResult("فشل في تحليل استجابة نون", transactionId);
			}

			if (noonResponse.ResultCode == NoonResultCode.Success && noonResponse.Result?.Order != null)
			{
				var order = noonResponse.Result.Order;
				var status = MapNoonStatus(order.Status);

				return new PaymentResult
				{
					Success = status == PaymentStatus.Completed,
					TransactionId = transactionId,
					Status = status,
					Metadata = new Dictionary<string, string>
					{
						["noonStatus"] = order.Status ?? "UNKNOWN",
						["orderReference"] = order.Reference ?? string.Empty
					}
				};
			}

			return CreateFailedResult(noonResponse.Message ?? "فشل في جلب حالة الدفع", transactionId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting Noon payment status for {TransactionId}", transactionId);
			return CreateFailedResult(ex.Message, transactionId);
		}
	}

	/// <inheritdoc />
	public async Task<RefundResult> RefundAsync(
		RefundRequest request,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Refunding Noon payment {TransactionId}, amount {Amount}",
				request.TransactionId, request.Amount);

			var noonRequest = new NoonRefundRequest
			{
				ApiOperation = "REFUND",
				Order = new NoonRefundOrderInfo
				{
					Id = request.TransactionId,
					Amount = request.Amount > 0 ? request.Amount : null // null = full refund
				}
			};

			var client = CreateHttpClient();
			var json = JsonSerializer.Serialize(noonRequest, _jsonOptions);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await client.PostAsync($"/order/{request.TransactionId}", content, cancellationToken);
			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

			_logger.LogDebug("Noon refund response: {Response}", responseContent);

			var noonResponse = JsonSerializer.Deserialize<NoonApiResponse>(responseContent, _jsonOptions);

			if (noonResponse?.ResultCode == NoonResultCode.Success)
			{
				_logger.LogInformation("Noon refund successful for {TransactionId}", request.TransactionId);

				return new RefundResult
				{
					Success = true,
					RefundId = noonResponse.Result?.Order?.Id ?? request.TransactionId
				};
			}

			return new RefundResult
			{
				Success = false,
				RefundId = string.Empty,
				ErrorMessage = noonResponse?.Message ?? "فشل في استرجاع المبلغ"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error refunding Noon payment {TransactionId}", request.TransactionId);
			return new RefundResult
			{
				Success = false,
				RefundId = string.Empty,
				ErrorMessage = ex.Message
			};
		}
	}

	/// <inheritdoc />
	public async Task<bool> CancelPaymentAsync(
		string transactionId,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Cancelling Noon payment {TransactionId}", transactionId);

			var noonRequest = new NoonCancelRequest
			{
				ApiOperation = "REVERSE"
			};

			var client = CreateHttpClient();
			var json = JsonSerializer.Serialize(noonRequest, _jsonOptions);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await client.PostAsync($"/order/{transactionId}", content, cancellationToken);
			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

			_logger.LogDebug("Noon cancel response: {Response}", responseContent);

			var noonResponse = JsonSerializer.Deserialize<NoonApiResponse>(responseContent, _jsonOptions);

			var success = noonResponse?.ResultCode == NoonResultCode.Success;
			_logger.LogInformation("Noon cancel result for {TransactionId}: {Success}", transactionId, success);

			return success;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error cancelling Noon payment {TransactionId}", transactionId);
			return false;
		}
	}

	/// <inheritdoc />
	public Task<bool> ValidateWebhookAsync(
		string payload,
		string signature,
		CancellationToken cancellationToken = default)
	{
		try
		{
			// التحقق من صحة الـ Webhook
			// نون يرسل توقيع في الـ header يجب التحقق منه
			// حالياً نقبل جميع الـ webhooks - يجب تنفيذ التحقق لاحقاً

			_logger.LogDebug("Validating Noon webhook. Signature: {Signature}", signature);

			if (string.IsNullOrEmpty(payload))
			{
				return Task.FromResult(false);
			}

			// محاولة تحليل الـ payload للتأكد من صحته
			var webhookData = JsonSerializer.Deserialize<NoonApiResponse>(payload, _jsonOptions);

			return Task.FromResult(webhookData != null);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error validating Noon webhook");
			return Task.FromResult(false);
		}
	}

	/// <summary>
	/// تأكيد عملية الدفع (للعمليات المحجوزة فقط)
	/// </summary>
	public async Task<PaymentResult> CapturePaymentAsync(
		string transactionId,
		decimal? amount = null,
		CancellationToken cancellationToken = default)
	{
		try
		{
			_logger.LogInformation("Capturing Noon payment {TransactionId}", transactionId);

			var noonRequest = new NoonCaptureRequest
			{
				ApiOperation = "CAPTURE",
				Order = new NoonCaptureOrderInfo
				{
					Amount = amount
				}
			};

			var client = CreateHttpClient();
			var json = JsonSerializer.Serialize(noonRequest, _jsonOptions);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await client.PostAsync($"/order/{transactionId}", content, cancellationToken);
			var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

			_logger.LogDebug("Noon capture response: {Response}", responseContent);

			var noonResponse = JsonSerializer.Deserialize<NoonApiResponse>(responseContent, _jsonOptions);

			if (noonResponse?.ResultCode == NoonResultCode.Success)
			{
				return new PaymentResult
				{
					Success = true,
					TransactionId = transactionId,
					Status = PaymentStatus.Completed
				};
			}

			return CreateFailedResult(noonResponse?.Message ?? "فشل في تأكيد الدفع", transactionId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error capturing Noon payment {TransactionId}", transactionId);
			return CreateFailedResult(ex.Message, transactionId);
		}
	}

	/// <summary>
	/// تحويل حالة نون إلى حالة الدفع العامة
	/// </summary>
	private static PaymentStatus MapNoonStatus(string? noonStatus)
	{
		return noonStatus?.ToUpperInvariant() switch
		{
			NoonOrderStatus.Initiated => PaymentStatus.Pending,
			NoonOrderStatus.Authorized => PaymentStatus.Processing,
			NoonOrderStatus.Captured => PaymentStatus.Completed,
			NoonOrderStatus.PaymentComplete => PaymentStatus.Completed,
			NoonOrderStatus.Failed => PaymentStatus.Failed,
			NoonOrderStatus.Cancelled => PaymentStatus.Cancelled,
			NoonOrderStatus.Reversed => PaymentStatus.Cancelled,
			NoonOrderStatus.Refunded => PaymentStatus.Refunded,
			NoonOrderStatus.PartiallyRefunded => PaymentStatus.PartiallyRefunded,
			NoonOrderStatus.Expired => PaymentStatus.Failed,
			_ => PaymentStatus.Pending
		};
	}

	/// <summary>
	/// إنشاء نتيجة فاشلة
	/// </summary>
	private static PaymentResult CreateFailedResult(string message, string transactionId = "")
	{
		return new PaymentResult
		{
			Success = false,
			TransactionId = transactionId,
			Status = PaymentStatus.Failed,
			ErrorMessage = message
		};
	}
}

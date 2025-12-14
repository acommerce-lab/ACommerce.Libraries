using ACommerce.Payments.Abstractions.Contracts;
using ACommerce.Payments.Abstractions.Models;
using ACommerce.Payments.Abstractions.Enums;
using ACommerce.Payments.Moyasar.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace ACommerce.Payments.Moyasar.Services;

/// <summary>
/// مزود دفع Moyasar
/// </summary>
public class MoyasarPaymentProvider : IPaymentProvider
{
	private readonly MoyasarOptions _options;
	private readonly IHttpClientFactory _httpClientFactory;

	public string ProviderName => "Moyasar";

	public MoyasarPaymentProvider(
		IOptions<MoyasarOptions> options,
		IHttpClientFactory httpClientFactory)
	{
		_options = options.Value;
		_httpClientFactory = httpClientFactory;
	}

	public async Task<PaymentResult> CreatePaymentAsync(
		PaymentRequest request,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var client = _httpClientFactory.CreateClient();
			client.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_options.ApiKey}:"))}");

			var payload = new
			{
				amount = (int)(request.Amount * 100), // convert to halalas
				currency = request.Currency.ToUpper(),
				description = request.Description ?? $"Order {request.OrderId}",
				callback_url = request.CallbackUrl,
				metadata = request.Metadata
			};

			var response = await client.PostAsJsonAsync(
				$"{_options.ApiUrl}/payments",
				payload,
				cancellationToken);

			if (response.IsSuccessStatusCode)
			{
				var result = await response.Content.ReadFromJsonAsync<dynamic>(cancellationToken);

				return new PaymentResult
				{
					Success = true,
					TransactionId = result?.id?.ToString() ?? string.Empty,
					Status = PaymentStatus.Pending,
					PaymentUrl = result?.source?.transaction_url?.ToString()
				};
			}

			var error = await response.Content.ReadAsStringAsync(cancellationToken);
			return new PaymentResult
			{
				Success = false,
				TransactionId = string.Empty,
				Status = PaymentStatus.Failed,
				ErrorMessage = error
			};
		}
		catch (Exception ex)
		{
			return new PaymentResult
			{
				Success = false,
				TransactionId = string.Empty,
				Status = PaymentStatus.Failed,
				ErrorMessage = ex.Message
			};
		}
	}

	public async Task<PaymentResult> GetPaymentStatusAsync(
		string transactionId,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var client = _httpClientFactory.CreateClient();
			client.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_options.ApiKey}:"))}");

			var response = await client.GetAsync(
				$"{_options.ApiUrl}/payments/{transactionId}",
				cancellationToken);

			if (response.IsSuccessStatusCode)
			{
				var result = await response.Content.ReadFromJsonAsync<dynamic>(cancellationToken);
				var status = result?.status?.ToString() ?? "pending";

				return new PaymentResult
				{
					Success = status == "paid",
					TransactionId = transactionId,
					Status = MapStatus(status)
				};
			}

			return new PaymentResult
			{
				Success = false,
				TransactionId = transactionId,
				Status = PaymentStatus.Failed
			};
		}
		catch
		{
			return new PaymentResult
			{
				Success = false,
				TransactionId = transactionId,
				Status = PaymentStatus.Failed
			};
		}
	}

	public async Task<RefundResult> RefundAsync(
		RefundRequest request,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var client = _httpClientFactory.CreateClient();
			client.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_options.ApiKey}:"))}");

			var payload = new
			{
				amount = (int)(request.Amount * 100)
			};

			var response = await client.PostAsJsonAsync(
				$"{_options.ApiUrl}/payments/{request.TransactionId}/refund",
				payload,
				cancellationToken);

			if (response.IsSuccessStatusCode)
			{
				var result = await response.Content.ReadFromJsonAsync<dynamic>(cancellationToken);

				return new RefundResult
				{
					Success = true,
					RefundId = result?.id?.ToString() ?? string.Empty
				};
			}

			var error = await response.Content.ReadAsStringAsync(cancellationToken);
			return new RefundResult
			{
				Success = false,
				RefundId = string.Empty,
				ErrorMessage = error
			};
		}
		catch (Exception ex)
		{
			return new RefundResult
			{
				Success = false,
				RefundId = string.Empty,
				ErrorMessage = ex.Message
			};
		}
	}

	public Task<bool> CancelPaymentAsync(
		string transactionId,
		CancellationToken cancellationToken = default)
	{
		// Moyasar doesn't support cancellation
		return Task.FromResult(false);
	}

	public Task<bool> ValidateWebhookAsync(
		string payload,
		string signature,
		CancellationToken cancellationToken = default)
	{
		// Implement webhook signature validation
		return Task.FromResult(true);
	}

	private static PaymentStatus MapStatus(string status)
	{
		return status?.ToLower() switch
		{
			"paid" => PaymentStatus.Completed,
			"failed" => PaymentStatus.Failed,
			"refunded" => PaymentStatus.Refunded,
			_ => PaymentStatus.Pending
		};
	}
}
